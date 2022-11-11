import { tags } from '@allors/default/workspace/meta';
import {
  AllorsMaterialDynamicCreateComponent as Create,
  AllorsMaterialDynamicEditComponent as Edit,
} from '@allors/base/workspace/angular-material/application';

export const dialogs = {
  create: {
    [tags.Person]: Create,
  },
  edit: {
    [tags.TaskAssignment]: Edit,
  },
};

export const components: any[] = [];
