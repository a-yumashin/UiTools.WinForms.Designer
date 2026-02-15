import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';
import { outputChannel } from './extension';

/**
 * Finds the nearest .csproj file by traversing up the directory tree.
 * @param startPath File or directory path to start search with
 * @returns Full path to the .csproj file or undefined.
 */
export function findContainingCsproj(startPath: string): string | undefined {
    // Determine the actual starting directory for the search:
    let searchDir = startPath;
    try {
        if (!fs.statSync(startPath).isDirectory()) {
            searchDir = path.dirname(startPath);
        }
    } catch (error: any) {
        // Error getting file/directory status (e.g. path doesn't exist or is inaccessible)
        outputChannel.appendLine(`[projectUtils.ts] Error getting status for path '${startPath}': ${error.stack || error}`);
        return undefined; // path is invalid or inaccessible
    }

    const workspaceRoot = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
    let currentDir = searchDir;
    while (currentDir && workspaceRoot && currentDir.startsWith(workspaceRoot)) {
        const files = fs.readdirSync(currentDir);
        const csproj = files.find(f => f.endsWith('.csproj'));

        if (csproj) {
            const fullProjPath = path.join(currentDir, csproj);
            try {
                const content = fs.readFileSync(fullProjPath, 'utf8');

                // Check for SDK-style project
                if (content.includes("Sdk=\"Microsoft.NET.Sdk\"")) {
                    return fullProjPath;
                } else {
                    // Check for Legacy-style project
                    const relativeFilePath = path.relative(currentDir, startPath).replace(/\//g, '\\');
                    // Check if the file is explicitly included in the project
                    if (content.includes(`<Compile Include="${relativeFilePath}"`)) {
                        return fullProjPath;
                    }
                }
            } catch (error: any) {
                outputChannel.appendLine(`[projectUtils.ts] Error reading project file '${fullProjPath}': ${error.stack || error}`);
            }
        }
        currentDir = path.dirname(currentDir);
    }
    return undefined;
}

/**
 * Determines namespace for the generated files (.cs and .designer.cs).
 * @param projectFile Full path to the .csproj file
 * @param folderPath Folder containing the .cs and .designer.cs files
 * @returns Namespace
 */
export function getNamespace(projectFile: string | undefined, folderPath: string): string {
    if (!projectFile)
        return "GlobalNamespace"; // fallback if project file was not found

    try {
        const content = fs.readFileSync(projectFile, 'utf8');
        // Look for RootNamespace
        const rootNamespaceMatch = content.match(/<RootNamespace>(.*?)<\/RootNamespace>/i);
        const rootNamespace = rootNamespaceMatch
            ? rootNamespaceMatch[1].trim()
            : path.basename(projectFile, '.csproj');

        const projectDir = path.dirname(projectFile);
        const relativePath = path.relative(projectDir, folderPath);

        // If the files folder is the same as the project folder - namespace = RootNamespace:
        if (!relativePath || relativePath === "")
            return rootNamespace;

        // Generate "subnamespace" from the relative path:
        const subNamespace = relativePath.split(path.sep).filter(s => s).join('.');
        return `${rootNamespace}.${subNamespace}`;
    } catch (error: any) {
        outputChannel.appendLine(`[projectUtils.ts] Error determining namespace from '${projectFile}': ${error.stack || error}`);
        return "ErrorNamespace";
    }
}
