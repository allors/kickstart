import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import {
  Action,
  ContextService,
  ErrorService,
  Filter,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  SingletonId,
  Table,
  TableRow,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import {
  Facility,
  InternalOrganisation,
  Locale,
  NonSerialisedInventoryItem,
  NonUnifiedPart,
  NonUnifiedPartBarcodePrint,
  Part,
  Person,
  ProductIdentificationType,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  DeleteActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  FetcherService,
  InternalOrganisationId,
  PrintService,
} from '@allors/apps-intranet/workspace/angular-material';
import { formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: Part;
  name: string;
  partNo: string;
  supplierNo: string;
  suppliers: string;
  categories: string;
  qoh: string;
  localQoh: string;
  brand: string;
  model: string;
  kind: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './nonunifiedpart-list-page.component.html',
  providers: [ContextService],
})
export class NonUnifiedPartListPageComponent implements OnInit, OnDestroy {
  public title = 'Spare Parts';

  table: Table<Row>;

  edit: Action;
  delete: Action;
  print: Action;

  private subscription: Subscription;
  goodIdentificationTypes: ProductIdentificationType[];
  parts: NonUnifiedPart[];
  nonUnifiedPartBarcodePrint: NonUnifiedPartBarcodePrint;
  facilities: Facility[];
  user: Person;
  internalOrganisation: InternalOrganisation;
  filter: Filter;
  m: M;
  sort: Sort;
  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public deleteService: DeleteActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public printService: PrintService,
    private errorService: ErrorService,
    private singletonId: SingletonId,
    private fetcher: FetcherService,
    public filterService: FilterService,
    public sorterService: SorterService,
    private internalOrganisationId: InternalOrganisationId,
    private userId: UserId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.print = printService.print();

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'partNo', sort: true },
        { name: 'name', sort: true },
        { name: 'supplierNo', sort: true },
        { name: 'suppliers', sort: true },
        { name: 'categories', sort: true },
        { name: 'qoh' },
        { name: 'localQoh' },
        { name: 'brand', sort: true },
        { name: 'model', sort: true },
        { name: 'kind', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.NonUnifiedPart);

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        scan(
          (
            [previousRefresh, previousFilterFields, , ,],
            [refresh, filterFields, sort, pageEvent, internalOrganisationId]
          ) => {
            pageEvent =
              previousRefresh !== refresh ||
              filterFields !== previousFilterFields
                ? {
                    ...pageEvent,
                    pageIndex: 0,
                  }
                : pageEvent;

            if (pageEvent.pageIndex === 0) {
              this.table.pageIndex = 0;
            }

            return [
              refresh,
              filterFields,
              sort,
              pageEvent,
              internalOrganisationId,
            ];
          }
        ),
        switchMap(
          ([, filterFields, sort, pageEvent, internalOrganisationId]: [
            Date,
            FilterField[],
            Sort,
            PageEvent,
            number
          ]) => {
            const pulls = [
              this.fetcher.internalOrganisation,
              pull.InternalOrganisation({
                objectId: internalOrganisationId,
                include: { FacilitiesWhereOwner: x },
              }),
              pull.NonUnifiedPart({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.NonUnifiedPart)?.create(sort)
                  : null,
                include: {
                  InventoryItemsWherePart: {
                    Facility: x,
                  },
                },
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
              pull.Singleton({
                name: 'PartBarcodePrint',
                objectId: this.singletonId.value,
                select: {
                  NonUnifiedPartBarcodePrint: {
                    include: {
                      Parts: x,
                      Facility: x,
                      Locale: x,
                      PrintDocument: {
                        Media: x,
                      },
                    },
                  },
                },
              }),
              pull.Locale({}),
              pull.ProductIdentificationType({}),
              pull.BasePrice({}),
              pull.Person({
                objectId: this.userId.value,
                include: {
                  UserProfile: {
                    DefaulLocale: x,
                  },
                },
              }),
            ];

            this.sort = sort;
            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.user = loaded.object<Person>(m.Person);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.facilities = loaded.collection<Facility>(m.Facility);
        this.nonUnifiedPartBarcodePrint =
          loaded.object<NonUnifiedPartBarcodePrint>('PartBarcodePrint');

        const locales = loaded.collection<Locale>(m.Locale);
        const dutchLocale = locales?.find((v) => v.Name === 'nl-NL');
        const spanishLocale = locales?.find((v) => v.Name === 'es-ES');

        this.parts = loaded.collection<NonUnifiedPart>(m.NonUnifiedPart);

        if (this.sort?.active === 'name') {
          if (this.user.Locale === spanishLocale) {
            if (this.sort.direction === 'asc') {
              this.parts = this.parts.sort((a, b) =>
                a.SpanishName > b.SpanishName
                  ? 1
                  : b.SpanishName > a.SpanishName
                  ? -1
                  : 1
              );
            } else {
              this.parts = this.parts.sort((a, b) =>
                a.SpanishName > b.SpanishName
                  ? 1
                  : b.SpanishName > a.SpanishName
                  ? 1
                  : -1
              );
            }
          } else if (this.user.Locale === dutchLocale) {
            if (this.sort.direction === 'asc') {
              this.parts = this.parts.sort((a, b) =>
                a.DutchName > b.DutchName
                  ? 1
                  : b.DutchName > a.DutchName
                  ? -1
                  : 1
              );
            } else {
              this.parts = this.parts.sort((a, b) =>
                a.DutchName > b.DutchName
                  ? 1
                  : b.DutchName > a.DutchName
                  ? 1
                  : -1
              );
            }
          } else {
            if (this.sort.direction === 'asc') {
              this.parts = this.parts.sort((a, b) =>
                a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 1
              );
            } else {
              this.parts = this.parts.sort((a, b) =>
                a.Name > b.Name ? 1 : b.Name > a.Name ? 1 : -1
              );
            }
          }
        }

        const inStockSearch = this.filter.fields?.find(
          (v) => v.definition.name === 'In Stock'
        );
        const outOfStockSearch = this.filter.fields?.find(
          (v) => v.definition.name === 'Out Of Stock'
        );
        let facilitySearchId = inStockSearch?.value;

        if (facilitySearchId === undefined) {
          facilitySearchId = outOfStockSearch?.value;
        }

        this.goodIdentificationTypes =
          loaded.collection<ProductIdentificationType>(
            m.ProductIdentificationType
          );

        this.table.data = this.parts?.map((v) => {
          const name =
            (this.user.UserProfile.DefaulLocale === dutchLocale
              ? v.DutchName
              : this.user.UserProfile.DefaulLocale === spanishLocale
              ? v.SpanishName
              : v.Name) ??
            v.Name ??
            v.DutchName ??
            v.SpanishName;

          return {
            object: v,
            name,
            partNo: v.ProductNumber,
            supplierNo: v.SupplierReferenceNumbers,
            suppliers: v.SuppliedByDisplayName,
            qoh: v.QuantityOnHand,
            localQoh:
              facilitySearchId &&
              (v.InventoryItemsWherePart as NonSerialisedInventoryItem[])?.find(
                (i) => i.Facility.id === facilitySearchId
              ).QuantityOnHand,
            categories: v.PartCategoriesDisplayName,
            brand: v.BrandName,
            model: v.ModelName,
            kind: v.InventoryItemKindName,
            lastModifiedDate: formatDistance(
              new Date(v.LastModifiedDate),
              new Date()
            ),
          } as Row;
        });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public printBarcode(parts: any): void {
    const { context } = this.allors;

    this.nonUnifiedPartBarcodePrint.Parts = parts;
    this.nonUnifiedPartBarcodePrint.Facility =
      this.internalOrganisation.FacilitiesWhereOwner[0];
    this.nonUnifiedPartBarcodePrint.Locale = this.user.Locale;

    this.allors.context.push().subscribe(() => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const pulls = [
        pull.Singleton({
          name: 'PartBarcodePrint',
          objectId: this.singletonId.value,
          select: {
            NonUnifiedPartBarcodePrint: {
              include: {
                PrintDocument: {
                  Media: x,
                },
              },
            },
          },
        }),
      ];

      this.allors.context.pull(pulls).subscribe((loaded) => {
        this.allors.context.reset();

        this.nonUnifiedPartBarcodePrint =
          loaded.object<NonUnifiedPartBarcodePrint>('PartBarcodePrint');

        this.print.execute(this.nonUnifiedPartBarcodePrint);
        this.refreshService.refresh();
      });
    }, this.errorService.errorHandler);
  }
}
