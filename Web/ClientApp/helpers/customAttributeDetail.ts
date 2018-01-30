import { CustomAttributeValueVM, CustomAttributeVM, GetCustomAttributeValueDetailVM } from '../apimodels/CustomAttributeValueStore';
import { CustomAttributeValueModel, CustomAttributeValueModelState, CustomAttributeValueDetail } from '../models/CustomAttributeValue';
import * as customAttributeValueHelpers from './customAttributeValues';

export const mapFromViewModel = (inputModel: GetCustomAttributeValueDetailVM): CustomAttributeValueDetail => {

    const { customAttribute, hasPermissionToApprove, productVariantName, topicPath } = inputModel;

    const current = inputModel.current || customAttributeValueHelpers.create();
    const draft = inputModel.draft || customAttributeValueHelpers.create();

    const currentMapped = customAttributeValueHelpers.mapFromApiModel(current);
    const draftMapped = customAttributeValueHelpers.mapFromApiModel(draft);

    return {
        current: currentMapped,
        draft: draftMapped,
        customAttribute,
        hasPermissionToApprove,
        productVariantName,
        topicPath
    }
}