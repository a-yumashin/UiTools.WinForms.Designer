import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';
import * as net from 'net';
import * as cp from 'child_process'; // for spawn
import { outputChannel } from './extension';
import * as packageJson from '../package.json';

let designerProcess: cp.ChildProcess | undefined;

/**
 * Forcibly terminates the designer process if it is running.
 */
export function stopDesigner() {
    if (designerProcess) {
        outputChannel.appendLine("[designerLauncher.ts] Stopping designer process...");
        designerProcess.kill();
        designerProcess = undefined;
    }
}

/**
 * Determines the absolute path to the UiTools.WinForms.Designer.exe which can be located either within
 * the VSIX package (for SelfContained build mode) or where the "uitools-winforms-designer.designerExePath"
 * Settings property points to (for Lightweight build mode or when overriding the built-in executable).
 * Displays an error message if the path cannot be resolved or the executable is not found.
 * @param extensionPath The absolute path to the root directory of the currently activating extension.
 * @returns The absolute path to the designer executable, or undefined if not found/accessible.
 */
export function getDesignerAppExecutablePath(extensionPath: string): string | undefined {
    // First check for the built-in EXE:
    const designerRelativePath = path.join('VSIX-bin', 'UiTools.WinForms.Designer.exe');
    const builtInPath = path.join(extensionPath, designerRelativePath);
    const hasBuiltIn = fs.existsSync(builtInPath);

    // Then check for EXE path in Settings:
    const config = vscode.workspace.getConfiguration('uitools-winforms-designer');
    const customPath = config.get<string>('designerExePath');
    if (customPath && customPath.trim() !== "") {
        if (fs.existsSync(customPath)) {
            if (hasBuiltIn) {
                // Both EXE files do exist - custom from Settings and the built-in one (SelfContained build mode):
                outputChannel.appendLine(
                    `[designerLauncher.ts] Using custom path to UiTools.WinForms.Designer.exe from Settings (overriding the built-in executable): ${customPath}`);
            } else {
                // No built-in EXE file found (Lightweight build mode):
                outputChannel.appendLine(
                    `[designerLauncher.ts] Using custom path to UiTools.WinForms.Designer.exe from Settings (no built-in executable found): ${customPath}`);
            }
            return customPath;
        } else {
            vscode.window.showWarningMessage(
                `Custom path to UiTools.WinForms.Designer.exe app not found: '${customPath}', will try to use the built-in executable...`);
        }
    }

    // We are here if custom path is not specified in Settings or is invalid:
    if (hasBuiltIn) {
        outputChannel.appendLine(
            `[designerLauncher.ts] Using the built-in UiTools.WinForms.Designer.exe app: ${builtInPath}`);
        return builtInPath;
    }

    // Neither custom nor built-in app is available:
    vscode.window.showErrorMessage(
        "UiTools.WinForms.Designer.exe app not found. Either provide a valid path in Settings or install the full " +
        "(self-contained) version of the extension (UiTools.WinForms.Designer.Full.vsix).", { modal: true });
    return undefined;
}

/**
 * Sends .designer.cs file full path to the UiTools.WinForms.Designer.exe application
 * (scenarios 2.1 and 2.2, see comments in the beginning of the UiTools.WinForms.Designer/Program.cs file)
 * @param designerCsFileFullPath Full path to the .designer.cs file
 * @param designerAppExePath Full path to the UiTools.WinForms.Designer.exe file
 */
export function sendToDesigner(designerCsFileFullPath: string, designerAppExePath: string) {
    const pipeName = '\\\\.\\pipe\\UiToolsWinFormsDesignerPipe'; // IMPORTANT: must be the same as in UiTools.WinForms.Designer.PipeServer.PIPE_NAME constant!

    // Helper function which tries to send .designer.cs file full path via Named Pipes
    const tryConnect = (isRetry: boolean = false) => {
        const client = net.connect(pipeName, () => {
            // --- Scenario 2.2 (or success after scenario 2.1) ---
            // Connection is established, server is listening.
            client.write(designerCsFileFullPath);
            client.end();
            vscode.window.showInformationMessage(isRetry
                ? `UiTools.WinForms.Designer.exe app was launched, data (path '${designerCsFileFullPath}') was passed.`
                : `Data (path '${designerCsFileFullPath}') was passed to a running UiTools.WinForms.Designer.exe app.`);
        });

        client.on('error', (err: any) => {
            if (!isRetry) {
                // --- Scenario 2.1 ---
                // Server not found --> UiTools.WinForms.Designer.exe app is probably not running --> let's try to start it:
                outputChannel.appendLine("[designerLauncher.ts] PipeServer not found, starting new instance...");
                try {
                    const args = [ '--vsix-version', packageJson.version ];
                    designerProcess = cp.spawn(designerAppExePath, args, {
                        detached: true,
                        stdio: 'ignore',
                        shell: false
                    });

                    designerProcess.on('exit', () => {
                        designerProcess = undefined;
                    });

                    designerProcess.unref();

                    // Wait a little (e.g. for 2 seconds) to let the UiTools.WinForms.Designer.exe app
                    // load itself and start its PipeServer, then try again:
                    setTimeout(() => tryConnect(true), 2000);

                } catch (spawnError: any) {
                    vscode.window.showErrorMessage(`Error starting designer: ${spawnError.message || spawnError}`);
                }
            } else {
                vscode.window.showErrorMessage("PipeServer not found even after it was started.");
            }
        });
    };

    // Do the first attempt:
    tryConnect();
}
