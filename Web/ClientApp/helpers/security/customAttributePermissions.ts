import { CustomAttributeValueDetail } from '../../models/CustomAttributeValue';

export const hasPermissionToUpdateCurrent = (customAttributeValue: CustomAttributeValueDetail) =>
    (customAttributeValue.current && customAttributeValue.hasPermissionToApprove)

export const hasPermissionToEditDraft = (customAttributeValue: CustomAttributeValueDetail, isDraftInEditMode: boolean) =>
    (customAttributeValue.draft.approved === null || isDraftInEditMode)

export const hasPermissionToSaveDraft = (customAttributeValue: CustomAttributeValueDetail, isDraftInEditMode: boolean) =>
    hasPermissionToEditDraft(customAttributeValue, isDraftInEditMode)

export const hasPermissionToApproveDraft = (customAttributeValue: CustomAttributeValueDetail, isDraftInEditMode: boolean) =>
    customAttributeValue.hasPermissionToApprove && hasPermissionToEditDraft(customAttributeValue, isDraftInEditMode)

export const hasPermissionToUnapproveDraft = (customAttributeValue: CustomAttributeValueDetail, isDraftInEditMode: boolean) =>
    customAttributeValue.hasPermissionToApprove && !hasPermissionToEditDraft(customAttributeValue, isDraftInEditMode)