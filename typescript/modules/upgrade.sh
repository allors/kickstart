cd ..

npx create-nx-workspace@latest allors --preset=empty --cli=nx --nx-cloud=false

cd allors

npm install -D jest-chain
npm install -D jest-trx-results-processor
npm install -D @nrwl/angular
npm install -D lnk
npm install -D @types/luxon

npm install @angular/cdk
npm install @angular/material
npm install @angular/material-luxon-adapter
npm install angular-calendar
npm install bootstrap@4.6.0
npm install common-tags
npm install date-fns
npm install easymde
npm install jsnlog
npm install luxon@2.5.0

// Intranet
npx nx g @nrwl/angular:application intranet --routing=true --style=scss --e2eTestRunner=none
npx nx g @nrwl/workspace:library intranet/workspace/angular-material
npx nx g @nrwl/workspace:library intranet/workspace/derivations
npx nx g @nrwl/workspace:library intranet/workspace/domain
npx nx g @nrwl/workspace:library intranet/workspace/meta
npx nx g @nrwl/workspace:library intranet/workspace/meta-json

// Extranet
npx nx g @nrwl/angular:application extranet --routing=true --style=scss --e2eTestRunner=none
npx nx g @nrwl/workspace:library extranet/workspace/angular-material
npx nx g @nrwl/workspace:library extranet/workspace/derivations
npx nx g @nrwl/workspace:library extranet/workspace/domain
npx nx g @nrwl/workspace:library extranet/workspace/meta
npx nx g @nrwl/workspace:library extranet/workspace/meta-json

// Base
npx nx g @nrwl/workspace:library base/workspace/angular/foundation
npx nx g @nrwl/workspace:library base/workspace/angular/application
npx nx g @nrwl/workspace:library base/workspace/angular-material/foundation
npx nx g @nrwl/workspace:library base/workspace/angular-material/application
npx nx g @nrwl/workspace:library base/workspace/derivations

// Core
npx nx g @nrwl/workspace:library core/workspace/derivations

// System
npx nx g @nrwl/workspace:library system/common/protocol-json
npx nx g @nrwl/workspace:library system/workspace/adapters
npx nx g @nrwl/workspace:library system/workspace/adapters-json
npx nx g @nrwl/workspace:library system/workspace/domain
npx nx g @nrwl/workspace:library system/workspace/meta
npx nx g @nrwl/workspace:library system/workspace/meta-json

