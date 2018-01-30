import { CustomAttributeValueCurrentIM, CustomAttributeValueDraftIM, CustomAttributeValueVM, CustomAttributeVM, CustomAttributeType } from '../apimodels/CustomAttributeValueStore';
import { CustomAttributeValueModel, CustomAttributeValueModelState } from '../models/CustomAttributeValue';

export const create = (): CustomAttributeValueModel => ({
    approved: null,
    approvedBy: null,
    customAttributeValueId: null,
    feedback: null,
    publishedFrom: null,
    satisfactionRatePercent: null,
    text: null,
    validFrom: null,
    valueBoolean: null,
    valueBooleanExtended: null,
    valueDateTime: null,
    valueDecimal: null,
    valueInt: null,
    valueIsIrrelevant: false,
    valueIsNotSet: true,
    valueState: CustomAttributeValueModelState.IsNotSet,
    valueString: null
})

export const mapToCurrentInputModel = (model: CustomAttributeValueModel, customAttributeId: number, productVariantId: number): CustomAttributeValueCurrentIM => {
    let valueIsIrrelevant = null;
    let valueIsNotSet = null;

    switch (model.valueState) {
        case CustomAttributeValueModelState.IsIrrelevant:
            valueIsIrrelevant = true;
            valueIsNotSet = true;
            break;
        case CustomAttributeValueModelState.IsNotSet:
            valueIsIrrelevant = false;
            valueIsNotSet = true;
            break;
        case CustomAttributeValueModelState.IsSet:
            valueIsIrrelevant = false;
            valueIsNotSet = false;
            break;
        default:
            throw Error(`Unknown custom attribute value model state ${model.valueState}`);
    }

    const customAttributeValueCurrent: CustomAttributeValueCurrentIM = {
        customAttributeId,
        productVariantId,
        feedback: model.feedback,
        satisfactionRatePercent: model.satisfactionRatePercent,
        text: model.text,
        valueBoolean: model.valueBoolean,
        valueBooleanExtended: model.valueBooleanExtended,
        valueDateTime: model.valueDateTime,
        valueDecimal: model.valueDecimal,
        valueInt: model.valueInt,
        valueIsIrrelevant,
        valueIsNotSet,
        valueString: model.valueString
    }

    return customAttributeValueCurrent;
}
export const mapToDraftInputModel = (model: CustomAttributeValueModel, approve: boolean, customAttributeId: number, productVariantId: number): CustomAttributeValueDraftIM => {
    let valueIsIrrelevant = null;
    let valueIsNotSet = null;

    switch (model.valueState) {
        case CustomAttributeValueModelState.IsIrrelevant:
            valueIsIrrelevant = true;
            valueIsNotSet = true;
            break;
        case CustomAttributeValueModelState.IsNotSet:
            valueIsIrrelevant = false;
            valueIsNotSet = true;
            break;
        case CustomAttributeValueModelState.IsSet:
            valueIsIrrelevant = false;
            valueIsNotSet = false;
            break;
        default:
            throw Error(`Unknown custom attribute value model state ${model.valueState}`);
    }

    const customAttributeValueDraft: CustomAttributeValueDraftIM = {
        customAttributeId,
        productVariantId,
        feedback: model.feedback,
        satisfactionRatePercent: model.satisfactionRatePercent,
        text: model.text,
        valueBoolean: model.valueBoolean,
        valueBooleanExtended: model.valueBooleanExtended,
        valueDateTime: model.valueDateTime,
        valueDecimal: model.valueDecimal,
        valueInt: model.valueInt,
        valueIsIrrelevant,
        valueIsNotSet,
        valueString: model.valueString,
        approve,
        publishedFrom: model.publishedFrom,
        validFrom: model.validFrom
    }

    return customAttributeValueDraft;
}

export const mapFromApiModel = (inputModel: CustomAttributeValueVM): CustomAttributeValueModel => {
    const valueState = (inputModel.valueIsIrrelevant ? CustomAttributeValueModelState.IsIrrelevant : (inputModel.valueIsNotSet ? CustomAttributeValueModelState.IsNotSet : CustomAttributeValueModelState.IsSet))
    return {...inputModel, valueState};
}

export const getCustomAttributeValueByType = (customAttributeType: CustomAttributeType, customAttributeValue: CustomAttributeValueModel): { key: string, value: string | number | Date | boolean } => {
    switch (customAttributeType) {
        case CustomAttributeType.Boolean:
            return { key: "valueBoolean", value: customAttributeValue.valueBoolean };

        case CustomAttributeType.BooleanExtended:
            return { key: "valueBooleanExtended", value: customAttributeValue.valueBooleanExtended };

        case CustomAttributeType.Date:
            return { key: "valueDate", value: customAttributeValue.valueDateTime };

        case CustomAttributeType.Decimal:
            return { key: "valueDecimal", value: customAttributeValue.valueDecimal };

        case CustomAttributeType.Integer:
            return { key: "valueInt", value: customAttributeValue.valueInt };

        case CustomAttributeType.Text:
            return { key: "valueString", value: customAttributeValue.valueString };

        default:
            throw new Error(`Unknown custom attribute type '${customAttributeType}'.`);
    }
}

export const getCustomAttributeTypeText = (type: CustomAttributeType) => {
    switch (type) {
        case CustomAttributeType.Boolean:
            return "Logická hodnota (Ano/Ne)"
        case CustomAttributeType.BooleanExtended:
            return "Rozšířená logická hodnota (Ano/Omezeně/Ne)"
        case CustomAttributeType.Date:
            return "Datum"
        case CustomAttributeType.Decimal:
            return "Desetinné číslo"
        case CustomAttributeType.Integer:
            return "Celé číslo"
        case CustomAttributeType.Text:
            return "Text"
        default:
            throw Error(`Unknown custom attribute type: ${type}`);
    }
}