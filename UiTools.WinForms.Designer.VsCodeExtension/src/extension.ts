import * as vscode from 'vscode';
import { addWinFormsItem } from './commands';
import { openInDesigner } from './commands';
import { stopDesigner } from './designerLauncher';

export const outputChannel = vscode.window.createOutputChannel("UiTools.WinForms.Designer");

export function activate(context: vscode.ExtensionContext) {
    outputChannel.appendLine('UiTools.WinForms.Designer extension is now active.');

    const openDesignerCommand = vscode.commands.registerCommand('uitools-winforms-designer.openInDesigner', async (item: any) => {
        await openInDesigner(item, context.extensionPath);
    });
    context.subscriptions.push(openDesignerCommand);

    const addFormCommand = vscode.commands.registerCommand('uitools-winforms-designer.addForm', async (uri: vscode.Uri) => {
        await addWinFormsItem(uri, 'Form');
    });
    context.subscriptions.push(addFormCommand);

    const addUserControlCommand = vscode.commands.registerCommand('uitools-winforms-designer.addUserControl', async (uri: vscode.Uri) => {
        await addWinFormsItem(uri, 'UserControl');
    });
    context.subscriptions.push(addUserControlCommand);
}

export function deactivate() {
    stopDesigner();
    outputChannel.appendLine('UiTools.WinForms.Designer extension is now inactive.');
}
