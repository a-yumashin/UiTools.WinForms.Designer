import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';
import { findContainingCsproj } from './projectUtils';
import { getNamespace } from './projectUtils';
import { sendToDesigner } from './designerLauncher';
import { getDesignerAppExecutablePath } from './designerLauncher';
import { outputChannel } from './extension';

/**
 * Opens given item (selected in VSCode Explorer, Solution Explorer or text editor) in the UiTools.WinForms.Designer.exe app.
 * @param item VSCode item to open in the UiTools.WinForms.Designer.exe app.
 * @param extensionPath Full path to the current extension's installation directory.
 */
export function openInDesigner(item: any, extensionPath: string) {
    let filePath: string | undefined;

    if (item) {
        if (item instanceof vscode.Uri) {
            filePath = item.fsPath;
        //} else if (item.fsPath) {
        //    filePath = item.fsPath;
        } else if (item.resourceUri) { // when called from the Solution Explorer
            filePath = item.resourceUri.fsPath;
        }
    } else { // when called from the Command Palette
        const activeEditor = vscode.window.activeTextEditor;
        if (activeEditor) {
            filePath = activeEditor.document.uri.fsPath;
        }
    }

    if (!filePath) {
        vscode.window.showWarningMessage("Please select a .designer.cs file in the Explorer or open it in an editor.");
        return;
    }

    if (!/\.designer\.cs$/i.test(filePath)) {
        vscode.window.showWarningMessage("Only .designer.cs files can be opened in the UiTools.WinForms.Designer.");
        return;
    }

    const designerCsPath = filePath;
    const mainCsPath = designerCsPath.replace(/\.designer\.cs$/i, '.cs');

    if (!fs.existsSync(mainCsPath)) {
        vscode.window.showErrorMessage(`Main file ${path.basename(mainCsPath)} not found.`);
        return;
    }
    try {
        const mainContent = fs.readFileSync(mainCsPath, 'utf8');
        const isWinForm = /:\s*(Form|UserControl)/.test(mainContent);
        if (!isWinForm) {
            // Additional check: sometimes the "main" (.cs) file does not contain inheritance
            // (when it is specified in another "part" of a partial class); however it's a rather rare scenario.
            vscode.window.showWarningMessage(`The file ${path.basename(mainCsPath)} does not explicitly inherit from Form or UserControl.`);
            return;
        }
    } catch (error: any) {
        vscode.window.showErrorMessage(`Error reading file ${mainCsPath}: ${error.message || error}`);
        return;
    }

    // Determine the path to the designer executable *inside* the VSIX package
    const designerAppExePath = getDesignerAppExecutablePath(extensionPath);
    if (!designerAppExePath) {
        return; // error message already shown by getDesignerAppExecutablePath()
    }

    sendToDesigner(designerCsPath, designerAppExePath); // use the resolved path
}

/**
 * Creates (generates) files (.cs and .designer.cs) for a new component of the given type ('Form' or 'UserControl') in the given folder.
 * @param item VSCode item - file or folder selected in VSCode Explorer or Solution Explorer.
 * @param type Type of the new component - either 'Form' or 'UserControl'
 */
export async function addWinFormsItem(item: any, type: 'Form' | 'UserControl') {
    let folderPath: string | undefined;

    if (item) {
        if (item instanceof vscode.Uri) {  // when called from the VSCode Explorer
            folderPath = item.fsPath;
        } else if (item.fsPath) { // when called from the Solution Explorer
            folderPath = item.fsPath;
        } else if (item.resourceUri) { // when called from the Solution Explorer
            folderPath = item.resourceUri.fsPath;
        } else if (typeof item.path === 'string') { // just for the case
            folderPath = item.path;
        }
    }

    if (folderPath) {
        try {
            folderPath = fs.statSync(folderPath).isDirectory()
                ? folderPath
                : path.dirname(folderPath); // if it's a file - take its folder
        } catch (error: any) {
            vscode.window.showErrorMessage(`Unable to access the selected folder '${folderPath}': ${error.message || error}`);
            outputChannel.appendLine(`[commands.ts] Error checking directory status for '${folderPath}': ${error.stack || error}`);
            return;
        }
    } else {
        // When called from the Command Palette (item === undefined)
        const activeEditor = vscode.window.activeTextEditor;
        if (activeEditor) {
            folderPath = path.dirname(activeEditor.document.uri.fsPath);
        }
    }

    if (!folderPath) {
        vscode.window.showErrorMessage("Couldn't determine folder to create file in.");
        return;
    }

    const name = await vscode.window.showInputBox({
        prompt: `Name for the new ${type}`,
        title: `Add ${type} (Windows Forms)...`,
        ignoreFocusOut: true
    });

    if (!name)
        return; // cancelled by user

    try {
        // Find .csproj file to determine the namespace to use:
        const projectFile = findContainingCsproj(folderPath);
        const namespace = getNamespace(projectFile, folderPath);

        const csPath = path.join(folderPath, `${name}.cs`);
        const designerCsPath = path.join(folderPath, `${name}.designer.cs`);

        // Check if the file(s) already exist:
        if (fs.existsSync(csPath) || fs.existsSync(designerCsPath)) {
            vscode.window.showErrorMessage(`File ${name}.cs or ${name}.designer.cs already exists!`);
            return;
        }

        const csContent = generateMainFile(namespace, name, type);
        const designerContent = generateDesignerFile(namespace, name, type);

        fs.writeFileSync(csPath, csContent);
        fs.writeFileSync(designerCsPath, designerContent);

        vscode.window.showInformationMessage(`${type} ${name} created successfully.`);
        // Open the "main" (.cs) file for editing:
        vscode.window.showTextDocument(await vscode.workspace.openTextDocument(csPath));

    } catch (error: any) {
        vscode.window.showErrorMessage(`Error creating ${type}: ${error.message || error}`);
        outputChannel.appendLine(`[commands.ts] Error creating ${type}: ${error.stack || error}`);
    }
}

/**
 * Generates source code (C#) of the "main" file (e.g. Form1.cs).
 * @param namespace Namespace to be used in source code.
 * @param name Name of the component (form/usercontrol) to be used in source code.
 * @param type Base type of the component (Form/UserControl) to be used in source code.
 * @returns Generated source code (C#) of the "main" file.
 */
function generateMainFile(namespace: string, name: string, type: string): string {
    return `using System;
using System.Windows.Forms;

namespace ${namespace}
{
    public partial class ${name} : ${type}
    {
        public ${name}()
        {
            InitializeComponent();
        }
    }
}`;
}

/**
 * Generates source code (C#) of the "designer" file (e.g. Form1.designer.cs).
 * @param namespace Namespace to be used in source code.
 * @param name Name of the component (form/usercontrol) to be used in source code.
 * @param type Base type of the component (Form/UserControl) to be used in source code.
 * @returns Generated source code (C#) of the "designer" file.
 */
function generateDesignerFile(namespace: string, name: string, type: string): string {
    return `namespace ${namespace}
{
    partial class ${name}
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ${type === 'Form' ? `this.ClientSize = new System.Drawing.Size(800, 450);` : ''}
            ${type === 'Form' ? `this.Text = "${name}";` : ''}
        }

        #endregion
    }
}`;
}
