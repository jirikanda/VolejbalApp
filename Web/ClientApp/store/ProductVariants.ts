import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import config from '../configuration/config';
import { Producer, ProductVariant } from '../apimodels/ProductVariantsStore';
import { toastr, actions } from 'react-redux-toastr';
import { Toastr } from './commonActionCreators/Toaster';

export interface ProductVariantsState {
    producers: Producer[];
    selectedProductVariants: ProductVariant[];
}

export enum ProductVariantsActionTypes {
    SelectProductVariant = 'SELECT_PRODUCT_VARIANT',
    UnselectProductVariant = 'UNSELECT_PRODUCT_VARIANT',

    SelectProductVariants = 'SELECT_PRODUCT_VARIANTS',
    UnselectProductVariants = 'UNSELECT_PRODUCT_VARIANTS',

    RequestProductVariants = 'REQUEST_PRODUCT_VARIANTS',
    ReceiveProductVariants = 'RECEIVE_PRODUCT_VARIANTS'
}

interface SelectProductVariant {
    type: ProductVariantsActionTypes.SelectProductVariant,
    productVariant: ProductVariant;
}

interface UnselectProductVariant {
    type: ProductVariantsActionTypes.UnselectProductVariant,
    productVariant: ProductVariant;
}

interface SelectProductVariants {
    type: ProductVariantsActionTypes.SelectProductVariants,
    productVariants: ProductVariant[];
}

interface UnselectProductVariants {
    type: ProductVariantsActionTypes.UnselectProductVariants,
    productVariants: ProductVariant[];
}

interface RequestProductVariants {
    type: ProductVariantsActionTypes.RequestProductVariants
}

interface ReceiveProductVariants {
    type: ProductVariantsActionTypes.ReceiveProductVariants,
    producers: Producer[];
}

type ProductVariantAction =
    SelectProductVariant | UnselectProductVariant
    | SelectProductVariants | UnselectProductVariants
    | RequestProductVariants | ReceiveProductVariants;

export const actionCreators = {
    selectProductVariant: (productVariant: ProductVariant): AppThunkAction<ProductVariantAction> => (dispatch, getState) => {
        const state = getState();
        const productVariants = state.productVariants.producers.map(producer => producer.productVariants).reduce((a, b) => a.concat(b));
        const selectedProductVariants = state.productVariants.selectedProductVariants;

        if (productVariants.find(productVariantItem => productVariantItem.id === productVariant.id)
            && !selectedProductVariants.find(productVariantItem => productVariantItem.id === productVariant.id)) {
            dispatch({ type: ProductVariantsActionTypes.SelectProductVariant, productVariant })
        }
    },
    unselectProductVariant: (productVariant: ProductVariant): AppThunkAction<ProductVariantAction> => (dispatch, getState) => {
        const state = getState();
        const productVariants = state.productVariants.producers.map(producer => producer.productVariants).reduce((a, b) => a.concat(b));
        const selectedProductVariants = state.productVariants.selectedProductVariants;

        if (productVariants.find(productVariantItem => productVariantItem.id === productVariant.id)
            && selectedProductVariants.find(productVariantItem => productVariantItem.id === productVariant.id)) {
            dispatch({ type: ProductVariantsActionTypes.UnselectProductVariant, productVariant })
        }
    },
    selectProductVariants: (productVariants: ProductVariant[]): AppThunkAction<ProductVariantAction> => (dispatch, getState) => {
        const state = getState();
        const selectedProductVariants = state.productVariants.selectedProductVariants;
        const productVariantsSet = new Set(productVariants)
        selectedProductVariants.map(slectedProductVariant => productVariantsSet.add(slectedProductVariant));

        dispatch({ type: ProductVariantsActionTypes.SelectProductVariants, productVariants: Array.from(productVariantsSet) })
    },
    saveProducersSelection: (productTypeId: number): AppThunkAction<ProductVariantAction> => (dispatch, getState, superagent, responseHandler) => {
        const state = getState().productVariants;
        const selectedProducers = state.producers.filter(producer =>
            producer.productVariants.some(prodVariant =>
                state.selectedProductVariants.some(slectedProdVariant => slectedProdVariant.id === prodVariant.id)
            )
        );

        superagent
            .post(`/api/productvariantsstore/producttype/${productTypeId}/producerselections`)
            .send(selectedProducers.map(producer => producer.id))
            .then(responseHandler(response => Toastr.success({ message: 'Výběr byl uložen.' })))
            .catch(err => {
                console.log(err);
                // dispatch error
            })
    },
    loadProducersSelection: (productTypeId: number): AppThunkAction<ProductVariantAction> => (dispatch, getState, superagent, responseHandler) => {
        superagent
            .get(`/api/productvariantsstore/producttype/${productTypeId}/producerselections`)
            .then(responseHandler(response => {
                const state = getState().productVariants;
                const savedProducerIds: number[] = response.body as number[];

                const productVariants = state.producers
                    .filter(producer => savedProducerIds.some(id => id === producer.id))
                    .map(producer => producer.productVariants)
                    .reduce((a, b) => a.concat(b), []);

                if (productVariants.length > 0) {
                    dispatch({ type: ProductVariantsActionTypes.SelectProductVariants, productVariants })
                } else {
                    actionCreators.selectAllProductVariants()(dispatch, getState, superagent, responseHandler);
                }
            }))
            .catch(error => console.error(error));
    },
    unselectProductVariants: (productVariants: ProductVariant[]): AppThunkAction<ProductVariantAction> => (dispatch, getState) => {
        const state = getState();
        const selectedProductVariants = state.productVariants.selectedProductVariants;

        const productVariantsToUnselect = selectedProductVariants.filter(selectedProductVariant =>
            productVariants.map(productVariant => productVariant.id).indexOf(selectedProductVariant.id) < 0);

        dispatch({ type: ProductVariantsActionTypes.UnselectProductVariants, productVariants: productVariantsToUnselect })
    },
    requestProductVariants: (productTypeId: number, shouldLoadProducers: boolean): AppThunkAction<ProductVariantAction> => async (dispatch, getState, superagent, responseHandler) => {
        if (productTypeId) {
            dispatch({ type: ProductVariantsActionTypes.RequestProductVariants });

            const request = superagent
                .get(`/api/productvariantsstore/producttypes/${productTypeId}/productvariants`)
                .then(responseHandler(response => dispatch({ type: ProductVariantsActionTypes.ReceiveProductVariants, producers: response.body.producers })));

            if (shouldLoadProducers) {
                request.then(response => actionCreators.loadProducersSelection(productTypeId)(dispatch, getState, superagent, responseHandler));
            }
        }
    },
    selectAllProductVariants: (): AppThunkAction<ProductVariantAction> => (dispatch, getState) => {
        const state = getState().productVariants;

        const productVariants = state.producers.map(producer => producer.productVariants).reduce((a, b) => a.concat(b), []);

        dispatch({ type: ProductVariantsActionTypes.SelectProductVariants, productVariants })
    },
    unselectAllProductVariants: (): AppThunkAction<ProductVariantAction> => (dispatch, getState) => {
        dispatch({ type: ProductVariantsActionTypes.UnselectProductVariants, productVariants: [] })
    }
}

const unloadedState = { producers: [], selectedProductVariants: [] }

export const reducer: Reducer<ProductVariantsState> = (state: ProductVariantsState, action: ProductVariantAction) => {
    switch (action.type) {
        case ProductVariantsActionTypes.SelectProductVariant:
            return {
                producers: state.producers,
                selectedProductVariants: state.selectedProductVariants.concat(action.productVariant)
            }
        case ProductVariantsActionTypes.UnselectProductVariant:
            return {
                producers: state.producers,
                selectedProductVariants: state.selectedProductVariants.filter(productVariantItem => productVariantItem.id !== action.productVariant.id)
            }
        case ProductVariantsActionTypes.SelectProductVariants:
            return {
                producers: state.producers,
                selectedProductVariants: action.productVariants
            }
        case ProductVariantsActionTypes.UnselectProductVariants:
            return {
                producers: state.producers,
                selectedProductVariants: action.productVariants
            }
        case ProductVariantsActionTypes.RequestProductVariants:
            return {
                producers: state.producers,
                selectedProductVariants: state.selectedProductVariants
            }
        case ProductVariantsActionTypes.ReceiveProductVariants:
            return {
                producers: action.producers,
                selectedProductVariants: state.selectedProductVariants
            }
        default:
            const exhaustiveCheck: never = action;
    }
    return state || unloadedState;
}