#! /usr/bin/env node

const path = require('path');
const lnk = require('lnk');

function link(src, dst){
    const basename = path.basename(src);

    lnk([src], dst)
    .then(() => console.log(basename + ' linked') )
    .catch((e) =>  e.errno && e.errno != -4075 ? console.log(e) : console.log('already linked'))
}

// Aviation Intranet
link ('../../aviation/typescript/modules/libs/aviation-intranet/workspace/angular-material/src/lib', 'libs/aviation-intranet/workspace/angular-material/src');
link ('../../aviation/typescript/modules/libs/aviation-intranet/workspace/derivations/src/lib', 'libs/aviation-intranet/workspace/derivations/src');

// Aviation Extranet
link ('../../aviation/typescript/modules/libs/aviation-extranet/workspace/angular-material/src/lib', 'libs/aviation-extranet/workspace/angular-material/src');
link ('../../aviation/typescript/modules/libs/aviation-extranet/workspace/derivations/src/lib', 'libs/aviation-extranet/workspace/derivations/src');

// Apps Intranet
link ('../../allors/typescript/modules/libs/apps-intranet/workspace/angular-material/src/lib', 'libs/apps-intranet/workspace/angular-material/src');
link ('../../allors/typescript/modules/libs/apps-intranet/workspace/derivations/src/lib', 'libs/apps-intranet/workspace/derivations/src');

// Apps Extranet
link ('../../allors/typescript/modules/libs/apps-extranet/workspace/angular-material/src/lib', 'libs/apps-extranet/workspace/angular-material/src');
link ('../../allors/typescript/modules/libs/apps-extranet/workspace/derivations/src/lib', 'libs/apps-extranet/workspace/derivations/src');

// Base
link ('../../allors/typescript/modules/libs/base/workspace/angular/foundation/src/lib', 'libs/base/workspace/angular/foundation/src');
link ('../../allors/typescript/modules/libs/base/workspace/angular/application/src/lib', 'libs/base/workspace/angular/application/src');
link ('../../allors/typescript/modules/libs/base/workspace/angular-material/foundation/src/lib', 'libs/base/workspace/angular-material/foundation/src');
link ('../../allors/typescript/modules/libs/base/workspace/angular-material/application/src/lib', 'libs/base/workspace/angular-material/application/src');
link ('../../allors/typescript/modules/libs/base/workspace/derivations/src/lib', 'libs/base/workspace/derivations/src');

// Core
link ('../../allors/typescript/modules/libs/core/workspace/derivations/src/lib', 'libs/core/workspace/derivations/src');

// System
link ('../../allors/typescript/modules/libs/system/common/protocol-json/src/lib', 'libs/system/common/protocol-json/src');
link ('../../allors/typescript/modules/libs/system/workspace/adapters/src/lib', 'libs/system/workspace/adapters/src');
link ('../../allors/typescript/modules/libs/system/workspace/adapters-json/src/lib', 'libs/system/workspace/adapters-json/src');
link ('../../allors/typescript/modules/libs/system/workspace/domain/src/lib', 'libs/system/workspace/domain/src');
link ('../../allors/typescript/modules/libs/system/workspace/meta/src/lib', 'libs/system/workspace/meta/src');
link ('../../allors/typescript/modules/libs/system/workspace/meta-json/src/lib', 'libs/system/workspace/meta-json/src');
