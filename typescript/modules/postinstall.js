#! /usr/bin/env node

const path = require('path');
const lnk = require('lnk');

function link(src, dst){
    const basename = path.basename(src);

    lnk([src], dst)
    .then(() => console.log(basename + ' linked') )
    .catch((e) =>  e.errno && e.errno != -4075 ? console.log(e) : console.log('already linked'))
}

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
