// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, TransportKind } from 'vscode-languageclient/node';

// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
export async function activate(context: vscode.ExtensionContext) {

	const serverPath = 'C:\\Code\\StationScriptCli\\ClrScript.LS\\bin\\Debug\\net9.0\\ClrScript.LS.exe';

	const serverOptions: ServerOptions = {
        run: { command: serverPath, transport: TransportKind.stdio },
        debug: { command: serverPath, transport: TransportKind.stdio }
    };

	    const clientOptions: LanguageClientOptions = {
        documentSelector: [{ scheme: 'file', language: 'mylanguage' }]
    };

    const client = new LanguageClient('clrScript', 'ClrScript Server', serverOptions, clientOptions);
    await client.start();


	// Use the console to output diagnostic information (console.log) and errors (console.error)
	// This line of code will only be executed once when your extension is activated
	console.log('Congratulations, your extension "clrscript" is now active!');

	// The command has been defined in the package.json file
	// Now provide the implementation of the command with registerCommand
	// The commandId parameter must match the command field in package.json
	const disposable = vscode.commands.registerCommand('clrscript.helloWorld', () => {
		// The code you place here will be executed every time your command is executed
		// Display a message box to the user
		vscode.window.showInformationMessage('Hello World from ClrScript!');
	});

	context.subscriptions.push(disposable);
}

// This method is called when your extension is deactivated
export function deactivate() {}


function getBundledServerPath(context: vscode.ExtensionContext): string {
    const platform = process.platform;
    switch (platform) {
        case 'win32':
            return context.asAbsolutePath('server/ClrScript.LS.exe');
        case 'linux':
        case 'darwin':
            return context.asAbsolutePath('server/MyLanguageServer');
        default:
            return context.asAbsolutePath('server/MyLanguageServer.exe');
    }
}
