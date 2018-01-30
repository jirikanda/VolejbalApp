import {
    DocumentsVM,
    PostDocumentIM,
    PostDocumentsIM,
    PostHistoricalDocumentIM
} from '../apimodels/ProductVariantDocumentStore';
import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import * as moment from 'moment';
import config from '../configuration/config';
import { methodologyDocuments, historicalMethodologyDocuments } from './fakes/MethodologyDocuments';
import { isDateInPast, getDateDiff } from '../helpers/momentHelpers';
import { ProductVariant } from './../apimodels/ProductMatrixStore';
import { Toastr } from './commonActionCreators/Toaster';

export interface ProductVariantDocument {
    id?: number;
    content?: string;
    name: string;
    validFrom: Date;
    validTo?: Date;
    url?: string;
}

export interface ProductVariantDocumentFile {
    file: File;
    validFrom: Date;
    validTo?: Date;
}

export interface ProductVariantDocumentsState {
    productVariantDocuments: ProductVariantDocument[];
    historicalProductVariantDocuments: ProductVariantDocument[];
    selectedProductVariant: ProductVariant;
}

export enum ProductVariantDocumentsTypes {
    RequestProductVariantDocuments = 'REQUEST_PRODUCT_VARIANT_DOCUMENTS',
    ReceiveProductVariantDocuments = 'RECEIVE_PRODUCT_VARIANT_DOCUMENTS',
    DeleteProductVariantDocument = 'DELETE_PRODUCT_VARIANT_DOCUMENTS',
    SaveProductVariantDocumentsMetadata = 'SAVE_PRODUCT_VARIANT_DOCUMENTS_METADATA',
    ChangeProductVariantDocumentsOrder = 'CHANGE_PRODUCT_VARIANT_DOCUMENTS_ORDER',
    AddProductVariantDocumentFile = 'ADD_PRODUCT_VARIANT_DOCUMENT_FILE'
}

interface AddProductVariantDocumentFile {
    type: ProductVariantDocumentsTypes.AddProductVariantDocumentFile,
    documentsIncludingAddedDocumet: ProductVariantDocument[];
}

interface ChangeProductVariantDocumentsOrder {
    type: ProductVariantDocumentsTypes.ChangeProductVariantDocumentsOrder,
    productVariantDocumentsWithNewOrder: ProductVariantDocument[];
}

interface SaveProductVariantDocumentsMetadata {
    type: ProductVariantDocumentsTypes.SaveProductVariantDocumentsMetadata;
    productVariantDocuments: ProductVariantDocument[];
}

interface RequestProductVariantDocuments {
    type: ProductVariantDocumentsTypes.RequestProductVariantDocuments;
}

interface ReceiveProductVariantDocuments {
    type: ProductVariantDocumentsTypes.ReceiveProductVariantDocuments;
    productVariantDocuments: ProductVariantDocument[];
    historicalProductVariantDocuments: ProductVariantDocument[];
    selectedProductVariant: ProductVariant;
}

interface DeleteProductVariantDocuments {
    type: ProductVariantDocumentsTypes.DeleteProductVariantDocument;
    documentsExcludingDeletedDocument: ProductVariantDocument[];
    isDocumentHistorical: boolean;
}

type ProductTypesAction =
    RequestProductVariantDocuments |
    ReceiveProductVariantDocuments |
    DeleteProductVariantDocuments |
    SaveProductVariantDocumentsMetadata |
    ChangeProductVariantDocumentsOrder |
    AddProductVariantDocumentFile;

export const actionCreators = {
    changeProductVariantDocumentsOrder: (hoverIndex: number, currentItemId: number): AppThunkAction<ProductTypesAction> => (dispatch, getState) => {
        const documents = getState().productVariantDocuments.productVariantDocuments;

        const productVariantDocumentsWithNewOrder: ProductVariantDocument[] = documents.filter(document => document.id !== currentItemId);
        const dragedDocument = documents.find(document => document.id === currentItemId);

        if(dragedDocument) {
            productVariantDocumentsWithNewOrder.splice(hoverIndex, 0, dragedDocument);

            dispatch({
                type: ProductVariantDocumentsTypes.ChangeProductVariantDocumentsOrder,
                productVariantDocumentsWithNewOrder
            });
        }
    },
    addProductVariantDocumentFile: (newDocumentFile: ProductVariantDocumentFile): AppThunkAction<ProductTypesAction> => (dispatch, getState, superagent) => {
        const state = getState().productVariantDocuments;
        const documents = state.productVariantDocuments;
        // because of drag&drop functionality, we have to generate temporary Ids for new documents
        // - we use negative values and right before upload to the server we have to replace them with null
        const newDocumentGeneratedId = (documents.filter(document => document.id < 0).length + 1) * -1;

        const reader = new FileReader();
        reader.onload = (e) => {
            const base64Content = reader.result.split(',')[1];  // we have to cut out "data:image/png;base64," from the beginning
            const documentsIncludingNewOne: ProductVariantDocument[] = [
                {
                    id: newDocumentGeneratedId,
                    name: newDocumentFile.file.name,
                    content: base64Content,
                    validFrom: newDocumentFile.validFrom,
                    validTo: newDocumentFile.validTo
                },
                ...documents
            ];

            dispatch({
                type: ProductVariantDocumentsTypes.AddProductVariantDocumentFile,
                documentsIncludingAddedDocumet: documentsIncludingNewOne
            });
        }
        reader.readAsDataURL(newDocumentFile.file);
    },
    uploadProductVariantDocuments: (): AppThunkAction<ProductTypesAction> => (dispatch, getState, superagent, responseHandler) => {
        const state = getState().productVariantDocuments;

        const documents = state.productVariantDocuments.map(document => {
            const postDocumentIM: PostDocumentIM = {
                content: document.content,
                id: document.id > 0 ? document.id : null,
                name: document.name,
                validFrom: document.validFrom,
                validTo: document.validTo
            };

            return postDocumentIM;
        })
        const historicalDocuments = state.historicalProductVariantDocuments.map(document => {
            const postHistoricalDocumentIM: PostHistoricalDocumentIM = { id: document.id };
            return postHistoricalDocumentIM;
        });

        const requestData: PostDocumentsIM = {
            documents,
            historicalDocuments
        }

        const request = superagent
            .post(`/api/ProductVariantDocumentStore/${state.selectedProductVariant.id}/documents`)
            .send(requestData)
            .then(responseHandler(
                response => {
                    Toastr.success({ message: 'Změny byly uloženy' });
                    superagent
                        .get(`/api/productvariantdocumentstore/${state.selectedProductVariant.id}/documents`)
                        .then(responseHandler(responseData => {
                            const documentsVM: DocumentsVM = responseData.body;
                            dispatch({
                                type: ProductVariantDocumentsTypes.ReceiveProductVariantDocuments,
                                productVariantDocuments: documentsVM.documents,
                                historicalProductVariantDocuments: documentsVM.historicalDocuments,
                                selectedProductVariant: state.selectedProductVariant
                            });
                        })).catch(reason => console.warn(reason));

                    dispatch({ type: ProductVariantDocumentsTypes.RequestProductVariantDocuments });
                },
                response => Toastr.error({ message: response.body.message }),
                response => Toastr.error({ message: 'Změny se nepodařilo uložit' })))
            .catch(reason => console.warn(reason.response.body));
    },
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument): AppThunkAction<ProductTypesAction> => (dispatch, getState) => {
        const isDocumentHistorical = productVariantDocument.validTo && isDateInPast(productVariantDocument.validTo);
        const documents: ProductVariantDocument[] = productVariantDocument.validTo && isDocumentHistorical
            ? getState().productVariantDocuments.historicalProductVariantDocuments
            : getState().productVariantDocuments.productVariantDocuments;

        const documentsExcludingDeletedDocument = documents.filter(document => document.id !== productVariantDocument.id);

        dispatch({
            type: ProductVariantDocumentsTypes.DeleteProductVariantDocument,
            documentsExcludingDeletedDocument,
            isDocumentHistorical
        });
    },
    saveProductVariantDocumentsMetadata: (productVariantDocument: ProductVariantDocument, newValidFrom: Date, newValidTo?: Date): AppThunkAction<ProductTypesAction> => (dispatch, getState) => {
        let documents: ProductVariantDocument[] = getState().productVariantDocuments.productVariantDocuments

        documents = documents.map(document => {
            if(document.id === productVariantDocument.id) {
                return {
                    ...productVariantDocument,
                    validFrom: newValidFrom,
                    validTo: newValidTo
                }
            }
            return document;
        })

        dispatch({
            type: ProductVariantDocumentsTypes.SaveProductVariantDocumentsMetadata,
            productVariantDocuments: documents
        });
    },
    requestProductVariantDocuments: (selectedProductVariant: ProductVariant): AppThunkAction<ProductTypesAction> => (dispatch, getState, superagent, responseHandler) => {
        const request = superagent
            .get(`/api/productvariantdocumentstore/${selectedProductVariant.id}/documents`)
            .then(responseHandler(response => {
                const documentsVM: DocumentsVM = response.body;
                dispatch({
                    type: ProductVariantDocumentsTypes.ReceiveProductVariantDocuments,
                    productVariantDocuments: documentsVM.documents,
                    historicalProductVariantDocuments: documentsVM.historicalDocuments,
                    selectedProductVariant
                });
            })).catch(reason => console.warn(reason));

        dispatch({ type: ProductVariantDocumentsTypes.RequestProductVariantDocuments });
    }
}

const unloadedProductTypesState: ProductVariantDocumentsState = { productVariantDocuments: [], historicalProductVariantDocuments: [], selectedProductVariant: null };

export const reducer: Reducer<ProductVariantDocumentsState> = (state: ProductVariantDocumentsState, action: ProductTypesAction) => {
    switch (action.type) {
        case ProductVariantDocumentsTypes.AddProductVariantDocumentFile:
            return {
                ...state,
                productVariantDocuments: action.documentsIncludingAddedDocumet
            };
        case ProductVariantDocumentsTypes.RequestProductVariantDocuments:
            return {
                ...state
            };
        case ProductVariantDocumentsTypes.DeleteProductVariantDocument:
            return {
                ...state,
                productVariantDocuments: action.isDocumentHistorical ? state.productVariantDocuments : action.documentsExcludingDeletedDocument,
                historicalProductVariantDocuments: action.isDocumentHistorical ? action.documentsExcludingDeletedDocument : state.historicalProductVariantDocuments
            };
        case ProductVariantDocumentsTypes.ReceiveProductVariantDocuments:
            return {
                productVariantDocuments: action.productVariantDocuments,
                historicalProductVariantDocuments: action.historicalProductVariantDocuments,
                selectedProductVariant: action.selectedProductVariant
            };
        case ProductVariantDocumentsTypes.SaveProductVariantDocumentsMetadata:
            return {
                ...state,
                productVariantDocuments: action.productVariantDocuments
            };
        case ProductVariantDocumentsTypes.ChangeProductVariantDocumentsOrder:
            return {
                ...state,
                productVariantDocuments: action.productVariantDocumentsWithNewOrder
            };
        default:
            const exhaustiveCheck: never = action;
    }

    return state || unloadedProductTypesState;
};