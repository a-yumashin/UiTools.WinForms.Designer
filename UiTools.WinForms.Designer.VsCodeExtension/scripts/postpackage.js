/*
 * As far as 'LICENSE.txt' file has been copied to project root folder only for packaging purposes - we must remove it from there
 * after packaging is done (because this folder already contains this file BUT as a link to solution-level 'LICENSE.txt', and
 * it's not a good idea to substitute this link with a real file forever)
 */
const fs = require('fs');
const path = require('path');

const projectRoot = path.join(__dirname, '..'); // project root (folder where package.json file resides)
const licensePath = path.join(projectRoot, 'LICENSE.txt');

try {
    if (fs.existsSync(licensePath)) {
        fs.unlinkSync(licensePath);
        console.log('Successfully removed temporary LICENSE.txt after packaging.');
    }
} catch (err) {
    console.error('Warning: Could not remove temporary LICENSE.txt: ' + err.message);
}
