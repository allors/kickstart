import { M, TreeBuilder } from '@allors/default/workspace/meta';
import { And } from '@allors/system/workspace/domain';
import { SearchFactory } from '@allors/base/workspace/angular/foundation';

export class CustomFilters {
  static iataFilter(m: M) {
    return new SearchFactory({
      objectType: m.IataGseCode,
      roleTypes: [m.IataGseCode.Code, m.IataGseCode.Name],
    });
  }

  static customerRelationshipsFilter(
    m: M,
    treeFactory: TreeBuilder,
    internalOrganisationId: number
  ) {
    return new SearchFactory({
      objectType: m.CustomerRelationship,
      roleTypes: [m.CustomerRelationship.CustomerName],
      include: treeFactory.CustomerRelationship({
        Agreements: {},
      }),
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.CustomerRelationship.InternalOrganisation,
              value: internalOrganisationId,
            },
            {
              kind: 'LessThan',
              roleType: m.CustomerRelationship.FromDate,
              value: new Date(),
            },
            {
              kind: 'Or',
              operands: [
                {
                  kind: 'Not',
                  operand: {
                    kind: 'Exists',
                    propertyType: m.CustomerRelationship.ThroughDate,
                  },
                },
                {
                  kind: 'GreaterThan',
                  roleType: m.CustomerRelationship.ThroughDate,
                  value: new Date(),
                },
              ],
            },
          ],
        });
      },
    });
  }
}
