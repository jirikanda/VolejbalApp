import { AppThunkAction } from './';
import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { CustomAttributeVM, CustomAttribute } from '../apimodels/TopicsStore';
import { initialize, getFormValues } from 'redux-form';
import { SuperAgent, SuperAgentRequest } from 'superagent';
import { CustomAttributeType } from '../apimodels/CustomAttributeValueStore';
import { Toastr } from './commonActionCreators/Toaster';

export interface CustomAttributeState {
    customAttributeVM: CustomAttributeVM;
}

export enum CustomAttributeActionTypes {
    RequestCustomAttribute = 'REQUEST_CUSTOMATTRIBUTE',
    ReceiveCustomAttribute = 'RECEIVE_CUSTOMATTRIBUTE',
    ClearCustomAttribute = 'CLEAR_CUSTOMATTRIBUTE',
    AddCustomAttribute = 'ADD_CUSTOMATTRIBUTE',
    RequestDeleteCustomAttribute = 'REQUEST_DELETE_CUSTOMATTRIBUTE',
    ReceiveDeleteCustomAttribute = 'RECEIVE_DELETE_CUSTOMATTRIBUTE',
    RequestSaveCustomAttribute = 'REQUEST_SAVE_CUSTOMATTRIBUTE',
    ReceiveSaveCustomAttribute = 'RECEIVE_SAVE_CUSTOMATTRIBUTE'
}

interface RequestCustomAttribute {
    type: CustomAttributeActionTypes.RequestCustomAttribute
}

interface ReceiveCustomAttribute {
    type: CustomAttributeActionTypes.ReceiveCustomAttribute,
    customAttributeVM: CustomAttributeVM;
}

interface ClearCustomAttribute {
    type: CustomAttributeActionTypes.ClearCustomAttribute;
}

interface AddCustomAttribute {
    type: CustomAttributeActionTypes.AddCustomAttribute;
    customAttributeVM: CustomAttributeVM;
}

interface RequestDeleteCustomAttribute {
    type: CustomAttributeActionTypes.RequestDeleteCustomAttribute;
}

interface ReceiveDeleteCustomAttribute {
    type: CustomAttributeActionTypes.ReceiveDeleteCustomAttribute;
}

interface RequestSaveCustomAttribute {
    type: CustomAttributeActionTypes.RequestSaveCustomAttribute;
}

interface ReceiveSaveCustomAttribute {
    type: CustomAttributeActionTypes.ReceiveSaveCustomAttribute;
    newCustomAttributeId: number;
}

type CustomAttributeAction =
    RequestCustomAttribute
    | ReceiveCustomAttribute
    | ClearCustomAttribute
    | AddCustomAttribute
    | RequestDeleteCustomAttribute
    | ReceiveDeleteCustomAttribute
    | RequestSaveCustomAttribute
    | ReceiveSaveCustomAttribute;

export const actionCreators = {
    requestCustomAttribute: (formName: string, productTypeId: number, customAttributeId: number): AppThunkAction<CustomAttributeAction> => (dispatch, getState, superagent, responseHandler) => {
        dispatch({ type: CustomAttributeActionTypes.RequestCustomAttribute });

        superagent.get(`/api/topicsstore/producttypes/${productTypeId}/customattribute/${customAttributeId}`)
            .then(responseHandler(
                response => {
                    dispatch({ type: CustomAttributeActionTypes.ReceiveCustomAttribute, customAttributeVM: response.body });
                    dispatch(initialize(formName, response.body));
                }))
            .catch(reason => console.warn(reason));
    },
    saveCustomAttribute: (formName: string, productTypeId: number, topicId: number, callback: () => void): AppThunkAction<CustomAttributeAction> => (dispatch, getState, superagent, responseHandler) => {
        dispatch({ type: CustomAttributeActionTypes.RequestSaveCustomAttribute });

        const state = getState();

        const inputModel = getFormValues(formName)(state) as CustomAttributeVM;
        const customAttributeId = state.customAttributes.customAttributeVM.id;

        let request: SuperAgentRequest;

        if (customAttributeId === null) {
            request = superagent.post(`/api/topicsstore/producttypes/${productTypeId}/customattribute`);
            inputModel.topicId = state.topics.topicVM.id;
        } else {
            request = superagent.put(`/api/topicsstore/producttypes/${productTypeId}/topics/customattribute/${customAttributeId}`);
        }

        request.send(inputModel)
            .then(responseHandler(
                response => {
                    dispatch({ type: CustomAttributeActionTypes.ReceiveSaveCustomAttribute, newCustomAttributeId: response.body });
                    callback();
                    Toastr.success({ message: "Upřesňující podmínka byla vytvořena." })
                }))
            .catch(reason => console.warn(reason));
    },
    clearCustomAttribute: (): AppThunkAction<CustomAttributeAction> => async (dispatch, getState, superagent, responseHandler) => dispatch({ type: CustomAttributeActionTypes.ClearCustomAttribute }),
    addCustomAttribute: (formName: string): AppThunkAction<CustomAttributeAction> => async (dispatch, getState, superagent, responseHandler) => {
        const newCustomAttribute = {
            id: null,
            includedInQuickSelection: false,
            measureUnitId: null,
            name: "",
            topicId: null,
            type: CustomAttributeType.Text,
            usesSatisfactionRate: false
        }
        dispatch(initialize(formName, newCustomAttribute));
        dispatch({ type: CustomAttributeActionTypes.AddCustomAttribute, customAttributeVM: newCustomAttribute })
    },
    deleteCustomAttribute: (productTypeId: number, callback: () => void): AppThunkAction<CustomAttributeAction> => (dispatch, getState, superagent, responseHandler) => {
        const customAttribute = getState().customAttributes.customAttributeVM

        dispatch({ type: CustomAttributeActionTypes.RequestDeleteCustomAttribute });

        if (customAttribute.id !== 0) {
            Toastr.confirm({
                message: `Opravdu chcete smazat upřesňující podmínku '${customAttribute.name}'?`,
                onOk: () => superagent.delete(`/api/topicsstore/producttypes/${productTypeId}/topics/customattribute/${customAttribute.id}`)
                    .then(responseHandler(
                        response => {
                            dispatch({ type: CustomAttributeActionTypes.ReceiveDeleteCustomAttribute });
                            callback();
                            Toastr.success({ message: "Upřesňující podmínka byla smazána." })
                        }))
                    .catch(reason => console.warn(reason))
            })            
        }
    }
}

const unloadedState = { customAttributeVM: null }

export const reducer: Reducer<CustomAttributeState> = (state: CustomAttributeState, action: CustomAttributeAction) => {
    switch (action.type) {
        case CustomAttributeActionTypes.RequestCustomAttribute:
        case CustomAttributeActionTypes.RequestDeleteCustomAttribute:
        case CustomAttributeActionTypes.RequestSaveCustomAttribute:
            return { ...state };

        case CustomAttributeActionTypes.ReceiveCustomAttribute:
            return {
                customAttributeVM: action.customAttributeVM
            }

        case CustomAttributeActionTypes.ReceiveSaveCustomAttribute:
            if (action.newCustomAttributeId) {
                return {
                    ...state,
                    customAttributeVM: { ...state.customAttributeVM, id: action.newCustomAttributeId }
                }
            }
            return { ...state };
            
        case CustomAttributeActionTypes.ClearCustomAttribute:
        case CustomAttributeActionTypes.ReceiveDeleteCustomAttribute:
            return { customAttributeVM: null }

        case CustomAttributeActionTypes.AddCustomAttribute:
            return {
                ...state,
                customAttributeVM: action.customAttributeVM
            }
        default:
            // The following line guarantees that every action in union has been covered by a case above
            const exhaustiveCheck: never = action;
    }
    return state || unloadedState
}