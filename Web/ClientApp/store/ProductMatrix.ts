import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { Topic } from './../apimodels/TopicsStore';
import { ProductVariant, CustomAttribute, PostFeedbackIM, PdfExportIM, PdfTemplate, PdfExportVM } from './../apimodels/ProductMatrixStore';
import { CustomAttributes } from './fakes/CustomAttributes';
import { Topics } from './fakes/Topics';
import { ProductType } from '../apimodels/SegmentsStore';
import { SatisfactionRateLevel, GetProductMatrixIM } from '../apimodels/ProductMatrixStore';
import { Toastr } from './commonActionCreators/Toaster';
import { getFlattenNodes } from '../helpers/hierarchy';

export interface ProductMatrixState {
    productVariants: ProductVariant[];
    productVariantsToHideByRateFilter: ProductVariant[];
    topics: Topic[];
    selectedTopics: Topic[];
    customAttributes: CustomAttribute[];
    selectedProductType: ProductType;
    satisfactionRateLevels: SatisfactionRateLevel[];
    satisfactionRateLevelFilters: SatisfactionRateLevelFilter[];
    isQuickSelection: boolean;
    currentSlideIndex: number;
    columnsToShow: number;
    PDFUrl: string;
    hasPermissionsToEdit: boolean;
}

export enum ProductMatrixActionTypes {
    RequestCustomAttributes = 'REQUEST_CUSTOM_ATTRIBUTES',
    ReceiveCustomAttributes = 'RECEIVE_CUSTOM_ATTRIBUTES',
    UnselectTopic = 'UNSELECT_TOPIC',
    UnselectProductVariant = 'UNSELECT_PRODUCT_VARIANT',
    ToggleSatisfactionRateLevelFilter = 'TOGGLE_SATISTFACTION_RATE_LEVEL_FILTER',
    SendFeedback = 'SEND_FEEDBACK',
    CancelSatisfactionRateLevelFilter = 'CANCEL_SATISTFACTION_RATE_LEVEL_FILTER',
    SetCurrentSlideIndex = 'SET_CURRENT_SLIDE_INDEX',
    SetColumnsToShow = 'SET_COLUMNS_TO_SHOW',
    SetPDFUrl = 'SET_PRODUCTMATRIX_PDFURL',
    ResetPDFUrl = 'RESET_PRODUCTMATRIX_PDFURL'
}

export interface SatisfactionRateLevelFilter {
    customAttribute: CustomAttribute;
    satisfactionRateLevels: SatisfactionRateLevel[];
}

interface RequestCustomAttributes {
    type: ProductMatrixActionTypes.RequestCustomAttributes;
    selectedProductType: ProductType;
}

interface ReceiveCustomAttributes {
    type: ProductMatrixActionTypes.ReceiveCustomAttributes;
    customAttributes: CustomAttribute[];
    productVariants: ProductVariant[];
    topics: Topic[];
    selectedTopics: Topic[];
    satisfactionRateLevels: SatisfactionRateLevel[];
    isQuickSelection: boolean;
}

interface UnselectTopic {
    type: ProductMatrixActionTypes.UnselectTopic;
    topic: Topic;
}

interface UnselectProductVariant {
    type: ProductMatrixActionTypes.UnselectProductVariant;
    productVariant: ProductVariant;
}

interface ToggleSatisfactionRateLevelFilter {
    type: ProductMatrixActionTypes.ToggleSatisfactionRateLevelFilter;
    rateLevel: SatisfactionRateLevel;
    customAttribute: CustomAttribute;
}

interface SendFeedback {
    type: ProductMatrixActionTypes.SendFeedback,
    feedback: string;
}

interface CancelSatisfactionRateLevelFilter {
    type: ProductMatrixActionTypes.CancelSatisfactionRateLevelFilter
}

interface SetCurrentSlideIndex {
    type: ProductMatrixActionTypes.SetCurrentSlideIndex;
    currentSlideIndex: number;
}

interface SetColumnsToShow {
    type: ProductMatrixActionTypes.SetColumnsToShow;
    columnsToShow: number;
}

interface SetPDFUrl {
    type: ProductMatrixActionTypes.SetPDFUrl;
    PDFUrl: string;
}

interface ResetPDFUrl {
    type: ProductMatrixActionTypes.ResetPDFUrl;
}

type ProductMatrixAction = RequestCustomAttributes
    | ReceiveCustomAttributes
    | UnselectTopic
    | UnselectProductVariant
    | ToggleSatisfactionRateLevelFilter
    | SendFeedback
    | CancelSatisfactionRateLevelFilter
    | SetCurrentSlideIndex
    | SetColumnsToShow
    | SetPDFUrl
    | ResetPDFUrl;

export const actionCreators = {
    exportToPDF: (template: PdfTemplate): AppThunkAction<ProductMatrixAction> => (dispatch, getState, superagent, responseHandler) => {
        const {productVariants, topics, productTypes} = getState();
        const productVariantIds = productVariants.selectedProductVariants.map(productVariant => productVariant.id);
        const customAttributeIds = topics.selectedTopics
            .map(topic => topic.customAttributes)
            .reduce((a, b) => a.concat(b), [])
            .map(customAttribute => customAttribute.id);

        const requestData: PdfExportIM = {
            "productVariantIds": productVariantIds,
            "customAttributeIds": customAttributeIds,
            "pdfTemplate": template
        }

        superagent
            .post(`/api/productmatrixstore/producttypes/${productTypes.productType.id}/pdfexport`)
            .send(requestData)
            .then(responseHandler(response => {
                if(response.ok) {
                    const responseData: PdfExportVM = response.body;
                    const pdfData = `data:application/octet-stream;base64, ${responseData.data}`;

                    dispatch({ type: ProductMatrixActionTypes.SetPDFUrl, PDFUrl: pdfData });
                }
            }))
            .catch(err => console.warn(err));
    },
    resetPDF: (): AppThunkAction<ProductMatrixAction> => (dispatch, getState, superagent, responseHandler) => {
        dispatch({ type: ProductMatrixActionTypes.ResetPDFUrl });
    },
    unselectTopic: (topic: Topic): AppThunkAction<ProductMatrixAction> => (dispatch, getState) => {
        dispatch({ type: ProductMatrixActionTypes.UnselectTopic, topic });
    },
    unselectProductVariant: (productVariant: ProductVariant): AppThunkAction<ProductMatrixAction> => (dispatch, getState) => {
        dispatch({ type: ProductMatrixActionTypes.UnselectProductVariant, productVariant });
    },
    requestCustomAttributes: (productTypeId: number): AppThunkAction<ProductMatrixAction> => (dispatch, getState, superagent, responseHandler) => {
        const appState = getState();
        const selectedProductVariants = appState.productVariants.selectedProductVariants;
        const selectedTopics = appState.topics.selectedTopics;
        const selectedProductType = appState.productTypes.productType;

        const requestData: GetProductMatrixIM = {
            topicIds: selectedTopics.map(topic => topic.id),
            productVariantIds: selectedProductVariants.map(variant => variant.id),
            limitToQuickSelection: false
        }
        const request = superagent
            .post(`/api/productmatrixstore/producttypes/${productTypeId}/productmatrix`)
            .send(requestData)
            .then(responseHandler(response => {
                dispatch({
                    type: ProductMatrixActionTypes.ReceiveCustomAttributes,
                    ...response.body,
                    selectedTopics,
                    isQuickSelection: false
                });
            }))
            .catch(err => console.warn(err));

        addTask(request); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: ProductMatrixActionTypes.RequestCustomAttributes, selectedProductType });
    },
    requestCustomAttributesQuickSelection: (productTypeId: number): AppThunkAction<ProductMatrixAction> => (dispatch, getState, superagent, responseHandler) => {
        const appState = getState();
        const selectedProductVariants = appState.productVariants.selectedProductVariants;
        const selectedTopics = appState.topics.selectedTopics;
        const selectedProductType = appState.productTypes.productType;

        const requestData: GetProductMatrixIM = {
            topicIds: null, // topics comes from web api
            productVariantIds: selectedProductVariants.map(variant => variant.id),
            limitToQuickSelection: true
        }
        const request = superagent
            .post(`/api/productmatrixstore/producttypes/${productTypeId}/productmatrix`)
            .send(requestData)
            .then(responseHandler(response => {
                const flattenTopics = getFlattenNodes(response.body.topics as Topic[]);
                dispatch({
                    type: ProductMatrixActionTypes.ReceiveCustomAttributes,
                    ...response.body,
                    selectedTopics: flattenTopics, // selected topics are topics from web api
                    isQuickSelection: true
                });
            }))
            .catch(err => console.warn(err));

        addTask(request); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: ProductMatrixActionTypes.RequestCustomAttributes, selectedProductType });
    },
    toggleSatisfactionRateLevel: (rateLevel: SatisfactionRateLevel, customAttribute: CustomAttribute): AppThunkAction<ProductMatrixAction> => (dispatch, getState) => {
        dispatch({ type: ProductMatrixActionTypes.ToggleSatisfactionRateLevelFilter, rateLevel, customAttribute });
    },
    sendFeedback: (productVariant: ProductVariant, customAttribute: CustomAttribute, feedback: string): AppThunkAction<ProductMatrixAction> => (dispatch, getState, superagent, responseHandler) => {
        const requestData: PostFeedbackIM = {
            productVariantId: productVariant.id,
            customAttributeId: customAttribute.id,
            feedback
        }

        superagent
            .post(`/api/productmatrixstore/producttypes/${getState().productMatrix.selectedProductType.id}/feedback`)
            .send(requestData)
            .then(responseHandler(
                response => Toastr.success({ message: 'E-mail se sdílenou zkušeností byl odeslán' }),
                response => Toastr.error({ message: response.body.message }),
                response => Toastr.error({ message: 'E-mail se sdílenou zkušeností se nepodařilo odeslat' })))
            .catch((err) => console.warn(err, err.response.body));
    },
    cancelSatisfactionRateLevel: (): AppThunkAction<ProductMatrixAction> => (dispatch, getState) => dispatch({ type: ProductMatrixActionTypes.CancelSatisfactionRateLevelFilter }),
    setCurrentSlideIndex: (currentSlideIndex: number): AppThunkAction<ProductMatrixAction> => (dispatch, getState) => dispatch({ type: ProductMatrixActionTypes.SetCurrentSlideIndex, currentSlideIndex }),
    setColumnsToShow: (columnsToShow: number): AppThunkAction<ProductMatrixAction> => (dispatch, getState) => dispatch({ type: ProductMatrixActionTypes.SetColumnsToShow, columnsToShow })
}

const unloadedState: ProductMatrixState = {
    productVariants: [],
    productVariantsToHideByRateFilter: [],
    topics: [],
    selectedTopics: [],
    customAttributes: [],
    selectedProductType: null,
    satisfactionRateLevels: [],
    satisfactionRateLevelFilters: [],
    isQuickSelection: false,
    currentSlideIndex: 0,
    columnsToShow: 0,
    PDFUrl: null,
    hasPermissionsToEdit: false
};

export const reducer: Reducer<ProductMatrixState> = (state: ProductMatrixState, action: ProductMatrixAction) => {
    switch (action.type) {
        case ProductMatrixActionTypes.SetPDFUrl:
            return {...state, PDFUrl: action.PDFUrl };
        case ProductMatrixActionTypes.ResetPDFUrl:
            return {...state, PDFUrl: null };
        case ProductMatrixActionTypes.RequestCustomAttributes:
            return {...state, selectedProductType: action.selectedProductType };

        case ProductMatrixActionTypes.ReceiveCustomAttributes:
            const filters = action.customAttributes.map(customAttribute => ({ customAttribute, satisfactionRateLevels: [] }))

            return {
                ...state,
                ...action,
                satisfactionRateLevelFilters: filters
            };

        case ProductMatrixActionTypes.UnselectTopic:
            return {...state, selectedTopics: state.selectedTopics.filter(topic => topic.id !== action.topic.id) }

        case ProductMatrixActionTypes.UnselectProductVariant:
            return {...state};

        case ProductMatrixActionTypes.ToggleSatisfactionRateLevelFilter:
            const existingFilter = state.satisfactionRateLevelFilters.find(filter => filter.customAttribute.id === action.customAttribute.id);
            const isRateLevelFiltered = existingFilter.satisfactionRateLevels.some(rateLevel => rateLevel.cssClass === action.rateLevel.cssClass);

            existingFilter.satisfactionRateLevels = isRateLevelFiltered
                ? existingFilter.satisfactionRateLevels.filter(filteredRateLevel => filteredRateLevel.cssClass !== action.rateLevel.cssClass)
                : existingFilter.satisfactionRateLevels.concat(action.rateLevel);

            const activeFilters = state.satisfactionRateLevelFilters.filter(filter => filter.satisfactionRateLevels.length > 0);

            const productVariantsToHide = activeFilters
                .map(activeFilter => activeFilter.customAttribute.values.filter(val => activeFilter.satisfactionRateLevels.some(rateLevel => val.satisfactionRateCssClass === rateLevel.cssClass))
                    .map(customAttributeValue => customAttributeValue.productVariantId))
                .reduce((a, b) => a.concat(b), [])
                .map(productVariantId => state.productVariants.find(productVariant => productVariant.id === productVariantId))

            return {...state, productVariantsToHideByRateFilter: productVariantsToHide }

        case ProductMatrixActionTypes.SendFeedback:
            // noop
            return { ...state };

        case ProductMatrixActionTypes.CancelSatisfactionRateLevelFilter:
            const cleanedSatisfactionRateLevelFilters = state.satisfactionRateLevelFilters.map(filter => ({ customAttribute: filter.customAttribute, satisfactionRateLevels: [] }));
            return {
                ...state,
                productVariantsToHideByRateFilter: [],
                satisfactionRateLevelFilters: cleanedSatisfactionRateLevelFilters
            };

        case ProductMatrixActionTypes.SetCurrentSlideIndex:
            return {
                ...state,
                currentSlideIndex: action.currentSlideIndex
            };

        case ProductMatrixActionTypes.SetColumnsToShow:
            return {
                ...state,
                columnsToShow: action.columnsToShow
            };

        default:
            // The following line guarantees that every action in union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState;
};