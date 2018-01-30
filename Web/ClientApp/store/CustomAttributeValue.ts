import { GetCustomAttributeValueDetailVM, CustomAttributeValueVM, CustomAttributeValueCurrentIM, CustomAttributeValueDraftIM } from '../apimodels/CustomAttributeValueStore'
import { CustomAttributeValueModel, CustomAttributeValueModelState, CustomAttributeValueDetail } from '../models/CustomAttributeValue';
import * as customAttributeValueHelpers from '../helpers/customAttributeValues';
import * as customAttributeDetailHelpers from '../helpers/customAttributeDetail';
import { AppThunkAction } from './';
import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { updateSyncErrors, getFormValues, SubmissionError, SubmissionErrorConstructor, initialize } from 'redux-form';
import { Toastr } from './commonActionCreators/Toaster';
import * as HttpStatusCodes from 'http-status-codes';
import { ValidationError } from '../apimodels/ValidationErrors';

export interface CustomAttributeValueState {
    customAttributeValue: CustomAttributeValueDetail;
    initialValues: CustomAttributeValueDetail;
    isDraftInEditMode: boolean;
    draftEditFormAction: DraftEditFormAction;
}

export enum CustomAttributeActionTypes {
    RequestCustomAttributeValue = 'REQUEST_CUSTOM_ATTRIBUTE_VALUE',
    RequestUpdateCurrentCustomAttributeValue = 'REQUEST_UPDATE_CURRENT_CUSTOM_ATTRIBUTE_VALUE',
    RequestSaveDraftCustomAttributeValue = 'REQUEST_NEW_DRAFT_CUSTOM_ATTRIBUTE_VALUE',
    RequestApproveDraftCustomAttributeValue = 'REQUEST_APPROVE_DRAFT_CUSTOM_ATTRIBUTE_VALUE',
    RequestUnapproveDraftCustomAttributeValue = 'REQUEST_UNAPPROVE_DRAFT_CUSTOM_ATTRIBUTE_VALUE',

    ReceiveCustomAttributeValue = 'RECEIVE_CUSTOM_ATTRIBUTE_VALUE',
    ReceiveUpdateCurrentCustomAttributeValue = 'RECEIVE_UPDATE_CURRENT_CUSTOM_ATTRIBUTE_VALUE',
    ReceiveSaveDraftCustomAttributeValue = 'RECEIVE_NEW_DRAFT_CUSTOM_ATTRIBUTE_VALUE',
    ReceiveApproveDraftCustomAttributeValue = 'RECEIVE_APPROVE_DRAFT_CUSTOM_ATTRIBUTE_VALUE',
    ReceiveUnapproveDraftCustomAttributeValue = 'RECEIVE_UNAPPROVE_DRAFT_CUSTOM_ATTRIBUTE_VALUE',

    SetCellEditFormAction = 'SET_CELL_EDIT_FORM_ACTION'
}

export enum DraftEditFormAction {
    SaveDraft,
    ApproveDraft,
    UnapproveDraft
}

// request actions
export interface RequestCustomAttributeValueAction {
    type: CustomAttributeActionTypes.RequestCustomAttributeValue
}
export interface RequestUpdateCurrentCustomAttributeValueAction {
    type: CustomAttributeActionTypes.RequestUpdateCurrentCustomAttributeValue
}
export interface RequestSaveDraftCustomAttributeValueAction {
    type: CustomAttributeActionTypes.RequestSaveDraftCustomAttributeValue
}
export interface RequestApproveDraftCustomAttributeValueAction {
    type: CustomAttributeActionTypes.RequestApproveDraftCustomAttributeValue
}
export interface RequestUnapproveDraftCustomAttributeValueAction {
    type: CustomAttributeActionTypes.RequestUnapproveDraftCustomAttributeValue
}

// response actions
export interface ReceiveCustomAttributeValueAction {
    type: CustomAttributeActionTypes.ReceiveCustomAttributeValue;
    customAttributeValue: CustomAttributeValueDetail;
}
export interface ReceiveUpdateCurrentCustomAttributeValueAction {
    type: CustomAttributeActionTypes.ReceiveUpdateCurrentCustomAttributeValue;
}
export interface ReceiveSaveDraftCustomAttributeValueAction {
    type: CustomAttributeActionTypes.ReceiveSaveDraftCustomAttributeValue;
}
export interface ReceiveApproveDraftCustomAttributeValueAction {
    type: CustomAttributeActionTypes.ReceiveApproveDraftCustomAttributeValue;
}
export interface ReceiveUnapproveDraftCustomAttributeValueAction {
    type: CustomAttributeActionTypes.ReceiveUnapproveDraftCustomAttributeValue;
}

export interface SetDraftFormAction {
    type: CustomAttributeActionTypes.SetCellEditFormAction;
    actionType: DraftEditFormAction;
}

type CustomAttributeValueAction = RequestCustomAttributeValueAction
    | RequestUpdateCurrentCustomAttributeValueAction
    | RequestSaveDraftCustomAttributeValueAction
    | RequestApproveDraftCustomAttributeValueAction
    | RequestUnapproveDraftCustomAttributeValueAction
    | ReceiveCustomAttributeValueAction
    | ReceiveUpdateCurrentCustomAttributeValueAction
    | ReceiveSaveDraftCustomAttributeValueAction
    | ReceiveApproveDraftCustomAttributeValueAction
    | ReceiveUnapproveDraftCustomAttributeValueAction
    | SetDraftFormAction;

export const actionCreators = {
    requestCustomAttributeValue: (productVariantId: number, customAttributeId: number, formNames: string[]): AppThunkAction<CustomAttributeValueAction> => (dispatch, getState, superagent, responseHandler) => {
        const request = superagent
            .post("/api/customattributevaluestore/values")
            .send({ productVariantId, customAttributeId })
            .then(responseHandler(response => {
                const model = customAttributeDetailHelpers.mapFromViewModel(response.body);
                formNames.forEach(formName => dispatch(initialize(formName, model)))
                dispatch({ type: CustomAttributeActionTypes.ReceiveCustomAttributeValue, customAttributeValue: model });
            }))

        addTask(request);
        dispatch({ type: CustomAttributeActionTypes.RequestCustomAttributeValue });
    },
    postUpdateCurrent: (formName: string, customAttributeId: number, productVariantId: number): AppThunkAction<CustomAttributeValueAction> => (dispatch, getState, superagent, responseHandler) => {
        const customAttributeValue = getFormValues(formName)(getState()) as CustomAttributeValueDetail;
        const requestBody = customAttributeValueHelpers.mapToCurrentInputModel(customAttributeValue.current, customAttributeId, productVariantId);        

        const request = superagent
            .post("/api/customattributevaluestore/values/current")
            .send(requestBody)
            .then(responseHandler(
                response => {
                    dispatch({ type: CustomAttributeActionTypes.ReceiveUpdateCurrentCustomAttributeValue });
                    Toastr.success({ message: 'Hodnoty aktualizovány.' });
                }
            ))
            .then(() => actionCreators.requestCustomAttributeValue(productVariantId, customAttributeId, [formName]))
            .catch(response => {
                const error = response.body as ValidationError;
                Toastr.error({ message: error.message });
            })

        addTask(request);
        dispatch({ type: CustomAttributeActionTypes.RequestUpdateCurrentCustomAttributeValue });
    },
    postSaveDraft: (formName: string, customAttributeId: number, productVariantId: number): AppThunkAction<CustomAttributeValueAction> => (dispatch, getState, superagent, responseHandler) => {
        const customAttributeValue = getFormValues(formName)(getState()) as CustomAttributeValueDetail;
        const requestInputModel = customAttributeValueHelpers.mapToDraftInputModel(customAttributeValue.draft, false, customAttributeId, productVariantId);

        const request = superagent
            .post("/api/customattributevaluestore/values/draft")
            .send(requestInputModel)
            .then(responseHandler(
                response => {
                    dispatch({ type: CustomAttributeActionTypes.ReceiveSaveDraftCustomAttributeValue });                    
                    Toastr.success({ message: 'Koncept uložen.' });
                }))
            .then(() => actionCreators.requestCustomAttributeValue(productVariantId, customAttributeId, [formName]));

        addTask(request);
        dispatch({ type: CustomAttributeActionTypes.RequestSaveDraftCustomAttributeValue });
    },
    postApproveDraft: (formName: string, customAttributeId: number, productVariantId: number): AppThunkAction<CustomAttributeValueAction> => (dispatch, getState, superagent, responseHandler) => {
        const customAttributeValue = getFormValues(formName)(getState()) as CustomAttributeValueDetail;
        const requestInputModel = customAttributeValueHelpers.mapToDraftInputModel(customAttributeValue.draft, true, customAttributeId, productVariantId);

        const request = superagent
            .post("/api/customattributevaluestore/values/draft")
            .send(requestInputModel)
            .then(responseHandler(response => {
                dispatch({ type: CustomAttributeActionTypes.ReceiveApproveDraftCustomAttributeValue });
                Toastr.success({ message: 'Koncept uložen a schválen.' });
            }))
            .then(() => actionCreators.requestCustomAttributeValue(productVariantId, customAttributeId, [formName]));

        addTask(request);
        dispatch({ type: CustomAttributeActionTypes.RequestApproveDraftCustomAttributeValue });
    },
    postUnapproveDraft: (formName: string, customAttributeValueId: number, customAttributeId: number, productVariantId: number): AppThunkAction<CustomAttributeValueAction> => (dispatch, getState, superagent, responseHandler) => {
        const request = superagent
            .post("/api/customattributevaluestore/values/draft/unapprove")
            .type("json")
            .send(String(customAttributeValueId))
            .then(responseHandler(response => {
                dispatch({ type: CustomAttributeActionTypes.ReceiveUnapproveDraftCustomAttributeValue });
                Toastr.success({ message: 'Koncept odemčen k editaci.' });
            }))
            .then(() => actionCreators.requestCustomAttributeValue(productVariantId, customAttributeId, [formName]));

        addTask(request);
        dispatch({ type: CustomAttributeActionTypes.RequestUnapproveDraftCustomAttributeValue });
    },
    setDraftEditFormAction: (action: DraftEditFormAction): AppThunkAction<CustomAttributeValueAction> => (dispatch, getState) => {
        dispatch({ type: CustomAttributeActionTypes.SetCellEditFormAction, actionType: action });
    }
}

const unloadedState: CustomAttributeValueState = {
    customAttributeValue: null,
    initialValues: null,
    isDraftInEditMode: false,
    draftEditFormAction: null
};

export const reducer: Reducer<CustomAttributeValueState> = (state: CustomAttributeValueState, action: CustomAttributeValueAction) => {
    switch (action.type) {
        case CustomAttributeActionTypes.RequestCustomAttributeValue:
            return { ...state }
        case CustomAttributeActionTypes.ReceiveCustomAttributeValue:
            return {
                ...state,
                customAttributeValue: action.customAttributeValue,
                initialValues: action.customAttributeValue
            };
        case CustomAttributeActionTypes.RequestUpdateCurrentCustomAttributeValue:
        case CustomAttributeActionTypes.RequestSaveDraftCustomAttributeValue:
        case CustomAttributeActionTypes.RequestApproveDraftCustomAttributeValue:
        case CustomAttributeActionTypes.RequestUnapproveDraftCustomAttributeValue:
            return { ...state }
        case CustomAttributeActionTypes.ReceiveUpdateCurrentCustomAttributeValue:
        case CustomAttributeActionTypes.ReceiveSaveDraftCustomAttributeValue:
            return {...state, isDraftInEditMode: true };
        case CustomAttributeActionTypes.ReceiveApproveDraftCustomAttributeValue:
            return {...state, isDraftInEditMode: false };
        case CustomAttributeActionTypes.ReceiveUnapproveDraftCustomAttributeValue:
            return {...state, isDraftInEditMode: true };
        case CustomAttributeActionTypes.SetCellEditFormAction:
            return {...state, draftEditFormAction: action.actionType };
        default:
            const exhaustiveCheck: never = action;
    }
    return state || unloadedState;
}