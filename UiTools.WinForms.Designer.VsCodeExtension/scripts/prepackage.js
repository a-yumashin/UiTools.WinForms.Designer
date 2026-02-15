const fs = require('fs');
const path = require('path');

/*
 * 1. We must either include the VSIX-bin folder in our package (in "full" mode) or
 *    exclude it from our package (in "light" mode).
 */
const isLight = process.argv.includes('--light'); // check for specific command line argument
const ignoreFile = path.join(__dirname, '..', '.vscodeignore');
const designerExePath = path.join(__dirname, '..', 'VSIX-bin', 'UiTools.WinForms.Designer.exe');

if (!isLight) {
    if (!fs.existsSync(designerExePath)) {
        console.error('--------------------------------------------------------------------');
        console.error('ERROR: UiTools.WinForms.Designer.exe not found in VSIX-bin folder!');
        console.error('You are trying to package a Full VSIX, but the binaries are missing.');
        console.error('Please rebuild the solution in a "SelfContained" configuration.');
        console.error('--------------------------------------------------------------------');
        process.exit(1); // STOP!
    }
}

// 1.1. At first let's ignore ALL files:
let ignoreContent = '**/*\n';

// 1.2. Now let's define the exclusions (i.e. those files which we ALLOW to package):
//      (in the .vscodeignore file, lines starting with symbol '!' are treated as "do NOT ignore")
const whitelist = [
    '!out/**',
    '!package.json',
    '!README.md',
    '!CHANGELOG.md',
    '!LICENSE.txt',
    '!icon.png'
];

// 1.3. Add the 'VSIX-bin' folder only if we are NOT in the "light"" mode:
if (!isLight) {
    console.log('Mode: Full. Including VSIX-bin in the package.');
    whitelist.push('!VSIX-bin/**');
} else {
    console.log('Mode: Light. Excluding VSIX-bin from the package.');
}

ignoreContent += whitelist.join('\n') + '\n';

// 1.4. Flush to disk:
fs.writeFileSync(ignoreFile, ignoreContent, 'utf8');
console.log('.vscodeignore has been updated.');

/*
 * 2. We must copy the 'LICENSE.txt' file to project root folder when creating our package.
 *    Solution Explorer shows this file in this folder but it's only a link to solution-level 'LICENSE.txt', so
 *    before packaging we must substitute it with a real file (because vsce tool does not resolve MSBuild links).
 */
const projectRoot = path.join(__dirname, '..');
const solutionRoot = path.join(projectRoot, '..');

const sourceLicense = path.join(solutionRoot, 'LICENSE.txt');
const destLicense = path.join(projectRoot, 'LICENSE.txt');

try {
    if (fs.existsSync(sourceLicense)) {
        fs.copyFileSync(sourceLicense, destLicense);
        console.log('Successfully copied LICENSE.txt from solution root.');
    } else {
        console.error('Source LICENSE.txt not found at: ' + sourceLicense);
        process.exit(1); // cancel packaging when license file is not available
    }
} catch (err) {
    console.error('Error copying LICENSE.txt: ' + err);
    process.exit(1); // cancel packaging when license file couldn't be copied
}
