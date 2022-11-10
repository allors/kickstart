import { OnInit, OnDestroy, Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription, of, empty, combineLatest, merge } from 'rxjs';
import {
  exhaustMap,
  share,
  filter,
  concatMap,
  catchError,
  map,
  switchMap,
  tap,
} from 'rxjs/operators';
import {
  ContextService,
  ErrorService,
  InvokeService,
  MediaService,
  RefreshService,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import {
  InventoryItem,
  Media,
  NonUnifiedPart,
  Person,
  ProductIdentification,
  SerialisedItem,
  TimeEntry,
  TimeSheet,
  WorkEffort,
  WorkEffortInventoryAssignment,
  WorkEffortPartyAssignment,
} from '@allors/default/workspace/domain';
import { AllorsCommandService } from '../../services/command/command.service';
import { M } from '@allors/default/workspace/meta';
import { Predicate } from '@allors/system/workspace/domain';
import {
  WorkEffortStateCreatedId,
  WorkEffortStateInProgressId,
} from '../constants';

@Component({
  templateUrl: 'shopfloor-app.component.html',
  styleUrls: ['shopfloor-app.component.scss'],
  providers: [ContextService],
})
export class ShopfloorAppComponent implements OnInit, OnDestroy {
  worker: Person;
  timeSheet: TimeSheet;
  runningTimeEntry: TimeEntry;
  lastTimeEntry: TimeEntry;
  workEffort: WorkEffort;
  equipment: SerialisedItem;
  inventoryAssignments: WorkEffortInventoryAssignment[];
  media: Media;
  isDutch: boolean;
  isSpanish: boolean;
  productIdentifications: ProductIdentification[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public mediaService: MediaService,
    public commandService: AllorsCommandService,
    private invokeService: InvokeService,
    private errorService: ErrorService,
    private route: ActivatedRoute,
    private userId: UserId,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const m = this.allors.context.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    const handleError = (e: any) => {
      const error = e instanceof Error ? e : new Error(e);

      this.snackBar.open(`⛔ ${error.name}: ${error.message}`, 'close', {
        duration: 5000,
      });

      return of(error);
    };

    const command$ = this.commandService.command$.pipe(
      exhaustMap((v) => of(v)),
      share()
    );

    const stopCommand$ = command$.pipe(
      filter((v) => v.name === 'stop' && this.runningTimeEntry != null),
      concatMap(() => {
        // Stop the running time entry
        this.runningTimeEntry.ThroughDate = new Date();
        return this.allors.context.push().pipe(catchError(handleError));
      })
    );

    const restartCommand$ = command$.pipe(
      filter(
        (v) =>
          v.name === 'restart' &&
          this.runningTimeEntry == null &&
          this.lastTimeEntry != null
      ),
      concatMap(() => {
        // Restart the last entry
        const newTimeEntry = this.allors.context.create(
          m.TimeEntry
        ) as TimeEntry;
        newTimeEntry.WorkEffort = this.lastTimeEntry.WorkEffort;
        newTimeEntry.FromDate = new Date();
        this.timeSheet.addTimeEntry(newTimeEntry);
        return this.allors.context.push().pipe(catchError(handleError));
      })
    );

    const barcodeCommand$ = command$.pipe(
      filter((v) => v.name === 'barcode'),
      map((v) => v.args[0]),
      concatMap((barcode) => {
        return this.allors.context
          .pull([
            pull.WorkEffort({
              predicate: {
                kind: 'Equals',
                propertyType: m.WorkEffort.WorkEffortNumber,
                value: barcode,
              },
            }),
            pull.ProductIdentification({
              predicate: {
                kind: 'Equals',
                propertyType: m.ProductIdentification.Identification,
                value: barcode,
              },
            }),
          ])
          .pipe(
            map((loaded) => {
              const workEfforts = loaded.collection<WorkEffort>(m.WorkEffort);
              const productIdentifications =
                loaded.collection<ProductIdentification>(
                  m.ProductIdentification
                );

              const workEffort =
                workEfforts?.length > 0 ? workEfforts[0] : null;
              const productIdentification =
                productIdentifications?.length > 0
                  ? productIdentifications[0]
                  : null;

              return workEffort ?? productIdentification;
            }),
            catchError(handleError),
            concatMap((v) => {
              if (v instanceof Error || v == null) {
                return empty();
              }

              return of(v);
            })
          );
      })
    );

    const workeffortBarcodeCommand$ = barcodeCommand$.pipe(
      filter((v) => m.WorkEffort.isAssignableFrom(v.strategy.cls)),
      concatMap((workEffort) => {
        return this.allors.context
          .pull([
            pull.WorkEffort({
              object: workEffort,
              include: {
                WorkEffortState: x,
                WorkEffortPartyAssignmentsWhereAssignment: {
                  Party: x,
                },
              },
            }),
          ])
          .pipe(
            concatMap((loaded) => {
              const workEffort = loaded.object<WorkEffort>(m.WorkEffort);
              const workEffortState = workEffort?.WorkEffortState;
              const isActive =
                workEffortState?.UniqueId == WorkEffortStateCreatedId ||
                workEffortState?.UniqueId == WorkEffortStateInProgressId;

              if (!isActive) {
                this.snackBar.open(
                  `⚠️ ${workEffort.WorkEffortNumber} ${workEffort.Name} is not active`,
                  'close',
                  {
                    duration: 5000,
                  }
                );
              } else {
                let assignment =
                  workEffort.WorkEffortPartyAssignmentsWhereAssignment.find(
                    (v) => v.Party === this.worker
                  );
                if (!assignment) {
                  assignment = this.allors.context.session.create(
                    m.WorkEffortPartyAssignment
                  ) as WorkEffortPartyAssignment;
                  assignment.Assignment = workEffort;
                  assignment.Party = this.worker;
                  assignment.FromDate = new Date();
                }

                if (workEffort !== this.runningTimeEntry?.WorkEffort) {
                  if (this.runningTimeEntry != null) {
                    // stop running time entry
                    this.runningTimeEntry.ThroughDate = new Date();
                  }

                  // create new time entry
                  const timeEntry = this.allors.context.create(
                    m.TimeEntry
                  ) as TimeEntry;
                  timeEntry.WorkEffort = workEffort;
                  timeEntry.FromDate = new Date();
                  this.timeSheet.addTimeEntry(timeEntry);

                  return this.allors.context
                    .push()
                    .pipe(catchError(handleError));
                }
              }

              return empty();
            }),
            catchError(handleError)
          );
      })
    );

    const partBarcodeCommand$ = barcodeCommand$.pipe(
      filter(
        (v) =>
          m.ProductIdentification.isAssignableFrom(v.strategy.cls) &&
          this.runningTimeEntry != null
      ),
      concatMap((productIdentification: ProductIdentification) => {
        const facilities = this.workEffort.TakenBy.FacilitiesWhereOwner;
        const facility =
          facilities && facilities.length > 0 ? facilities[0] : null;

        const predicate: Predicate = {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.InventoryItem.Facility,
              object: facility,
            },
            {
              kind: 'ContainedIn',
              propertyType: m.InventoryItem.Part,
              extent: {
                kind: 'Filter',
                objectType: m.UnifiedProduct,
                predicate: {
                  kind: 'Contains',
                  propertyType: m.UnifiedProduct.ProductIdentifications,
                  object: productIdentification,
                },
              },
            },
          ],
        };

        return this.allors.context
          .pull([
            pull.WorkEffortInventoryAssignment({
              predicate: {
                kind: 'And',
                operands: [
                  {
                    kind: 'Equals',
                    propertyType: m.WorkEffortInventoryAssignment.Assignment,
                    object: this.workEffort,
                  },
                  {
                    kind: 'ContainedIn',
                    propertyType: m.WorkEffortInventoryAssignment.InventoryItem,
                    extent: {
                      kind: 'Filter',
                      objectType: m.InventoryItem,
                      predicate,
                    },
                  },
                ],
              },
            }),
            pull.InventoryItem({
              predicate,
            }),
          ])
          .pipe(
            concatMap((loaded) => {
              const workEffortState = this.workEffort?.WorkEffortState;
              const isActive =
                workEffortState?.UniqueId == WorkEffortStateCreatedId ||
                workEffortState?.UniqueId == WorkEffortStateInProgressId;

              if (!isActive) {
                this.snackBar.open(
                  `⚠️ ${this.workEffort.WorkEffortNumber} ${this.workEffort.Name} is not active`,
                  'close',
                  {
                    duration: 5000,
                  }
                );
              } else {
                const inventoryAssignments =
                  loaded.collection<WorkEffortInventoryAssignment>(
                    m.WorkEffortInventoryAssignment
                  );
                let inventoryAssignment =
                  inventoryAssignments && inventoryAssignments.length > 0
                    ? inventoryAssignments[0]
                    : null;
                const inventoryItems = loaded.collection<InventoryItem>(
                  m.InventoryItem
                );
                const inventoryItem =
                  inventoryItems && inventoryItems.length > 0
                    ? inventoryItems[0]
                    : null;

                if (inventoryAssignment == null && inventoryItem == null) {
                  this.snackBar.open(`⚠️ No Inventory`, 'close', {
                    duration: 5000,
                  });
                } else {
                  if (inventoryAssignment != null) {
                    inventoryAssignment.Quantity = (
                      Number(inventoryAssignment.Quantity) + 1
                    ).toString();
                  } else {
                    inventoryAssignment = this.allors.context.create(
                      m.WorkEffortInventoryAssignment
                    ) as WorkEffortInventoryAssignment;
                    inventoryAssignment.Assignment = this.workEffort;
                    inventoryAssignment.InventoryItem = inventoryItem;
                    inventoryAssignment.Quantity = '1';
                  }

                  return this.allors.context
                    .push()
                    .pipe(catchError(handleError));
                }
              }

              return empty();
            }),
            catchError(handleError)
          );
      })
    );

    const load$ = combineLatest(
      this.route.url,
      this.userId.observable$,
      this.refreshService.refresh$
    ).pipe(
      switchMap(() => {
        const pulls = [
          pull.Person({
            objectId: this.userId.value,
            include: {
              UserProfile: {
                DefaulLocale: x,
                DefaultInternalOrganization: x,
              },
              Picture: x,
              TimeSheetWhereWorker: {
                TimeEntries: x,
              },
              LastTimeEntry: {
                WorkEffort: {
                  Customer: x,
                  WorkEffortState: x,
                  TakenBy: {
                    FacilitiesWhereOwner: x,
                  },
                  WorkEffortFixedAssetAssignmentsWhereAssignment: {
                    FixedAsset: {
                      SerialisedItem_PrimaryPhoto: x,
                      SerialisedItem_PartWhereSerialisedItem: {
                        PrimaryPhoto: x,
                        ProductIdentifications: x,
                      },
                    },
                  },
                  WorkEffortInventoryAssignmentsWhereAssignment: {
                    InventoryItem: {
                      Part: x,
                    },
                  },
                },
              },
            },
          }),
        ];

        return this.allors.context.pull(pulls).pipe(
          catchError((e) => {
            return of(e instanceof Error ? e : new Error(e));
          })
        );
      }),
      tap((loaded) => {
        this.allors.context.session.reset();

        if (loaded instanceof Error) {
          this.snackBar.open(`⛔ ${loaded.name}: ${loaded.message}`, 'close', {
            duration: 5000,
          });
        } else {
          this.worker = loaded.object<Person>(m.Person);
          this.timeSheet = this.worker.TimeSheetWhereWorker as TimeSheet;

          const workEffortState =
            this.worker.LastTimeEntry?.WorkEffort?.WorkEffortState;
          const isActive =
            workEffortState?.UniqueId == WorkEffortStateCreatedId ||
            workEffortState?.UniqueId == WorkEffortStateInProgressId;

          this.lastTimeEntry = isActive ? this.worker.LastTimeEntry : null;
          this.runningTimeEntry =
            this.lastTimeEntry?.ThroughDate == null ? this.lastTimeEntry : null;

          this.workEffort =
            this.runningTimeEntry?.WorkEffort ?? this.lastTimeEntry?.WorkEffort;
          this.equipment =
            this.workEffort?.WorkEffortFixedAssetAssignmentsWhereAssignment
              .length > 0
              ? (this.workEffort
                  .WorkEffortFixedAssetAssignmentsWhereAssignment[0]
                  .FixedAsset as SerialisedItem)
              : null;
          this.media =
            this.equipment?.PrimaryPhoto ??
            this.equipment?.PartWhereSerialisedItem?.PrimaryPhoto;
          this.productIdentifications =
            this.equipment?.PartWhereSerialisedItem?.ProductIdentifications ||
            [];

          this.inventoryAssignments =
            this.workEffort?.WorkEffortInventoryAssignmentsWhereAssignment;

          const locale = this.worker?.UserProfile?.DefaulLocale?.Name;
          this.isDutch = locale?.startsWith('nl');
          this.isSpanish = locale?.startsWith('es');
        }
      })
    );

    const commands$ = merge(
      stopCommand$,
      restartCommand$,
      workeffortBarcodeCommand$,
      partBarcodeCommand$
    ).pipe(
      tap(() => {
        this.refreshService.refresh();
      })
    );

    this.subscription = merge(commands$, load$).subscribe();
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  asNonUnifiedPart(part): NonUnifiedPart {
    return part;
  }
}
