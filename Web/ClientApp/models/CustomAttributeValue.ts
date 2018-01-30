import { CustomAttributeValueVM, CustomAttributeVM } from '../apimodels/CustomAttributeValueStore';

export interface CustomAttributeValueModel extends CustomAttributeValueVM {
    valueState: CustomAttributeValueModelState;
}

export enum CustomAttributeValueModelState {
    IsSet = 1,
    IsNotSet = 2,
    IsIrrelevant = 3
}

export interface CustomAttributeValueDetail {
    current: CustomAttributeValueModel;
    customAttribute: CustomAttributeVM;
    draft: CustomAttributeValueModel;
    hasPermissionToApprove: boolean;
    productVariantName: string;
    topicPath: string;
}
