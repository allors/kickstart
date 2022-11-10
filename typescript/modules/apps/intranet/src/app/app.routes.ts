import { Routes } from '@angular/router';
import { AuthorizationService } from './auth/authorization.service';

import { LoginComponent } from './auth/login.component';
import { ErrorComponent } from './error/error.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import {
  BrandListPageComponent,
  CarrierListPageComponent,
  CommunicationEventListPageComponent,
  CustomerShipmentOverviewPageComponent,
  EmailMessageListPageComponent,
  ExchangeRateListPageComponent,
  FacilityListPageComponent,
  GoodListPageComponent,
  ModelListPageComponent,
  NonUnifiedGoodOverviewPageComponent,
  NotificationListPageComponent,
  PartCategoryListPageComponent,
  PartListPageComponent,
  PositionTypeRateListPageComponent,
  PositionTypesListPageComponent,
  ProductQuoteListPageComponent,
  ProductTypesOverviewPageComponent,
  PurchaseInvoiceListPageComponent,
  PurchaseOrderListPageComponent,
  PurchaseReturnOverviewPageComponent,
  PurchaseShipmentOverviewPageComponent,
  RequestForQuoteListPageComponent,
  RequestForQuoteOverviewPageComponent,
  SalesInvoiceListPageComponent,
  SalesOrderListPageComponent,
  SerialisedItemCharacteristicListPageComponent,
  ShipmentListPageComponent,
  TaskAssignmentListPageComponent,
  UnifiedGoodListPageComponent,
  WorkRequirementOverviewPageComponent,
} from '@allors/apps-intranet/workspace/angular-material';

import {
  CataloguesListPageComponent,
  MaintenanceAgreementListPageComponent,
  MaintenanceAgreementOverviewPageComponent,
  NonUnifiedPartListPageComponent,
  NonUnifiedPartOverviewPageComponent,
  OrganisationListPageComponent,
  OrganisationOverviewPageComponent,
  PersonListPageComponent,
  PersonOverviewPageComponent,
  ProductCategoryListPageComponent,
  ProductQuoteOverviewPageComponent,
  PurchaseInvoiceOverviewPageComponent,
  PurchaseOrderOverviewPageComponent,
  SalesInvoiceOverviewPageComponent,
  SalesOrderOverviewPageComponent,
  SerialisedItemListPageComponent,
  SerialisedItemOverviewPageComponent,
  SettingsListPageComponent,
  UnifiedGoodOverviewPageComponent,
  VehicleListPageComponent,
  VehicleOverviewPageComponent,
  WorkEffortListPageComponent,
  WorkEffortTypeListPageComponent,
  WorkEffortTypeOverviewPageComponent,
  WorkRequirementListPageComponent,
  WorkTaskOverviewPageComponent,
  // Apps
  ShopfloorAppComponent,
  RedirectComponent,
  TimesheetAppComponent,
} from '@allors/aviation-intranet/workspace/angular-material';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'error', component: ErrorComponent },
  {
    canActivate: [AuthorizationService],
    path: '',
    children: [
      {
        path: '',
        component: RedirectComponent,
        pathMatch: 'full',
      },
      {
        path: 'shopfloor',
        component: ShopfloorAppComponent,
      },
      {
        path: '',
        component: MainComponent,
        children: [
          {
            path: 'dashboard',
            component: DashboardComponent,
            pathMatch: 'full',
          },
          {
            path: 'contacts',
            children: [
              { path: 'people', component: PersonListPageComponent },
              { path: 'person/:id', component: PersonOverviewPageComponent },
              {
                path: 'organisations',
                component: OrganisationListPageComponent,
              },
              {
                path: 'organisation/:id',
                component: OrganisationOverviewPageComponent,
              },
              {
                path: 'communicationevents',
                component: CommunicationEventListPageComponent,
              },
            ],
          },

          {
            path: 'sales',
            children: [
              {
                path: 'requestsforquote',
                component: RequestForQuoteListPageComponent,
              },
              {
                path: 'requestforquote/:id',
                component: RequestForQuoteOverviewPageComponent,
              },
              {
                path: 'productquotes',
                component: ProductQuoteListPageComponent,
              },
              {
                path: 'productquote/:id',
                component: ProductQuoteOverviewPageComponent,
              },
              { path: 'salesorders', component: SalesOrderListPageComponent },
              {
                path: 'salesorder/:id',
                component: SalesOrderOverviewPageComponent,
              },
              {
                path: 'salesinvoices',
                component: SalesInvoiceListPageComponent,
              },
              {
                path: 'salesinvoice/:id',
                component: SalesInvoiceOverviewPageComponent,
              },
            ],
          },
          {
            path: 'products',
            children: [
              { path: 'goods', component: GoodListPageComponent },
              {
                path: 'nonunifiedgood/:id',
                component: NonUnifiedGoodOverviewPageComponent,
              },
              { path: 'parts', component: NonUnifiedPartListPageComponent },
              {
                path: 'nonunifiedpart/:id',
                component: NonUnifiedPartOverviewPageComponent,
              },
              { path: 'unifiedgoods', component: UnifiedGoodListPageComponent },
              {
                path: 'unifiedgood/:id',
                component: UnifiedGoodOverviewPageComponent,
              },
              { path: 'vehicles', component: VehicleListPageComponent },
              { path: 'vehicle/:id', component: VehicleOverviewPageComponent },
              {
                path: 'partcategories',
                component: PartCategoryListPageComponent,
              },
              {
                path: 'productcategories',
                component: ProductCategoryListPageComponent,
              },
              { path: 'brands', component: BrandListPageComponent },
              { path: 'models', component: ModelListPageComponent },
              {
                path: 'producttypes',
                component: ProductTypesOverviewPageComponent,
              },
              {
                path: 'serialiseditems',
                component: SerialisedItemListPageComponent,
              },
              {
                path: 'serialisedItem/:id',
                component: SerialisedItemOverviewPageComponent,
              },
              {
                path: 'serialiseditemcharacteristics',
                component: SerialisedItemCharacteristicListPageComponent,
              },
              { path: 'facilities', component: FacilityListPageComponent },
            ],
          },
          {
            path: 'purchasing',
            children: [
              {
                path: 'purchaseorders',
                component: PurchaseOrderListPageComponent,
              },
              {
                path: 'purchaseorder/:id',
                component: PurchaseOrderOverviewPageComponent,
              },
              {
                path: 'purchaseinvoices',
                component: PurchaseInvoiceListPageComponent,
              },
              {
                path: 'purchaseinvoice/:id',
                component: PurchaseInvoiceOverviewPageComponent,
              },
              { path: 'purchasereturns', component: ShipmentListPageComponent },
            ],
          },

          {
            path: 'shipment',
            children: [
              { path: 'shipments', component: ShipmentListPageComponent },
              {
                path: 'customershipment/:id',
                component: CustomerShipmentOverviewPageComponent,
              },
              {
                path: 'purchasereturn/:id',
                component: PurchaseReturnOverviewPageComponent,
              },
              {
                path: 'purchaseshipment/:id',
                component: PurchaseShipmentOverviewPageComponent,
              },
              { path: 'carriers', component: CarrierListPageComponent },
            ],
          },

          {
            path: 'workefforts',
            children: [
              {
                path: 'workrequirements',
                component: WorkRequirementListPageComponent,
              },
              {
                path: 'workrequirement/:id',
                component: WorkRequirementOverviewPageComponent,
              },
              { path: 'workefforts', component: WorkEffortListPageComponent },
              {
                path: 'worktask/:id',
                component: WorkTaskOverviewPageComponent,
              },
              {
                path: 'maintenanceagreements',
                component: MaintenanceAgreementListPageComponent,
              },
              {
                path: 'maintenanceagreement/:id',
                component: MaintenanceAgreementOverviewPageComponent,
              },
              {
                path: 'workefforttypes',
                component: WorkEffortTypeListPageComponent,
              },
              {
                path: 'workefforttype/:id',
                component: WorkEffortTypeOverviewPageComponent,
              },
            ],
          },

          {
            path: 'humanresource',
            children: [
              {
                path: 'positiontypes',
                component: PositionTypesListPageComponent,
              },
              {
                path: 'positiontyperates',
                component: PositionTypeRateListPageComponent,
              },
            ],
          },

          {
            path: 'workflow',
            children: [
              {
                path: 'taskassignments',
                component: TaskAssignmentListPageComponent,
              },
              {
                path: 'notifications',
                component: NotificationListPageComponent,
              },
              {
                path: 'timesheet',
                component: TimesheetAppComponent,
              },
            ],
          },
          {
            path: 'accounting',
            children: [
              {
                path: 'exchangerates',
                component: ExchangeRateListPageComponent,
              },
            ],
          },
          {
            path: 'admin',
            children: [
              { path: 'settings', component: SettingsListPageComponent },
              {
                path: 'emailmessages',
                component: EmailMessageListPageComponent,
              },
            ],
          },
        ],
      },
    ],
  },
];

export const components: any[] = [
  LoginComponent,
  ErrorComponent,
  MainComponent,
  DashboardComponent,
  BrandListPageComponent,
  CarrierListPageComponent,
  CataloguesListPageComponent,
  CommunicationEventListPageComponent,
  CustomerShipmentOverviewPageComponent,
  EmailMessageListPageComponent,
  ExchangeRateListPageComponent,
  GoodListPageComponent,
  MaintenanceAgreementListPageComponent,
  MaintenanceAgreementOverviewPageComponent,
  NonUnifiedGoodOverviewPageComponent,
  NonUnifiedPartListPageComponent,
  NonUnifiedPartOverviewPageComponent,
  NotificationListPageComponent,
  OrganisationListPageComponent,
  OrganisationOverviewPageComponent,
  PartCategoryListPageComponent,
  PartListPageComponent,
  PersonListPageComponent,
  PersonOverviewPageComponent,
  PositionTypesListPageComponent,
  PositionTypeRateListPageComponent,
  ProductCategoryListPageComponent,
  ProductQuoteListPageComponent,
  ProductQuoteOverviewPageComponent,
  ProductTypesOverviewPageComponent,
  PurchaseInvoiceListPageComponent,
  PurchaseInvoiceOverviewPageComponent,
  PurchaseOrderListPageComponent,
  PurchaseOrderOverviewPageComponent,
  PurchaseShipmentOverviewPageComponent,
  RedirectComponent,
  RequestForQuoteListPageComponent,
  RequestForQuoteOverviewPageComponent,
  SalesInvoiceListPageComponent,
  SalesInvoiceOverviewPageComponent,
  SalesOrderListPageComponent,
  SalesOrderOverviewPageComponent,
  SerialisedItemCharacteristicListPageComponent,
  SerialisedItemListPageComponent,
  SerialisedItemOverviewPageComponent,
  SettingsListPageComponent,
  ShipmentListPageComponent,
  ShopfloorAppComponent,
  TaskAssignmentListPageComponent,
  TimesheetAppComponent,
  UnifiedGoodListPageComponent,
  UnifiedGoodOverviewPageComponent,
  VehicleListPageComponent,
  WorkEffortListPageComponent,
  WorkEffortTypeListPageComponent,
  WorkEffortTypeOverviewPageComponent,
  WorkRequirementListPageComponent,
  WorkRequirementOverviewPageComponent,
  WorkTaskOverviewPageComponent,
];
