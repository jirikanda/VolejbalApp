import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import config from '../configuration/config';
import * as SegmentStore from './Segments';
import { ProductType } from '../apimodels/SegmentsStore';

export interface ProductTypesState {
    productTypes: ProductType[];
    productType: ProductType;
}

export enum ProductTypesActionTypes {
    RequestProductTypes = 'REQUEST_PRODUCTTYPES',
    ReceiveProductTypes = 'RECEIVE_PRODUCTTYPES',
    SelectProductType = 'SELECT_PRODUCTTYPE'
}

interface RequestProductTypes {
    type: ProductTypesActionTypes.RequestProductTypes;
}

interface ReceiveProductTypes {
    type: ProductTypesActionTypes.ReceiveProductTypes;
    productTypes: ProductType[];
}

interface SelectProductType {
    type: ProductTypesActionTypes.SelectProductType,
    productType: ProductType
}

type ProductTypesAction = RequestProductTypes | ReceiveProductTypes | SelectProductType;

export const actionCreators = {
    requestProductTypes: (segmentId: number): AppThunkAction<ProductTypesAction> => (dispatch, getState, superagent, responseHandler) => {
        const request = superagent.get(`/api/producttypesstore/segments/${segmentId}/producttypes`)
            .then(responseHandler(
                response => dispatch({ type: ProductTypesActionTypes.ReceiveProductTypes, productTypes: response.body })))
            .catch(reason => console.warn(reason));

        addTask(request);
        dispatch({ type: ProductTypesActionTypes.RequestProductTypes });
    },
    selectProductType: (productType: ProductType): AppThunkAction<ProductTypesAction> => (dispatch, getState) => {
        dispatch({ type: ProductTypesActionTypes.SelectProductType, productType });
    }
}

const unloadedProductTypesState: ProductTypesState = { productTypes: [], productType: null };

export const reducer: Reducer<ProductTypesState> = (state: ProductTypesState, action: ProductTypesAction) => {
    switch (action.type) {
        case ProductTypesActionTypes.RequestProductTypes:
            return {
                productTypes: [],
                productType: null
            };
        case ProductTypesActionTypes.ReceiveProductTypes:
            return {
                productTypes: action.productTypes,
                productType: state.productType || action.productTypes[0]
            };
        case ProductTypesActionTypes.SelectProductType:
            return {
                productTypes: state.productTypes,
                productType: action.productType
            }
        default:
            const exhaustiveCheck: never = action;
    }

    return state || unloadedProductTypesState;
};