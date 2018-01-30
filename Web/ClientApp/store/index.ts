import * as CodebooksStore from './Codebooks'
import * as ErrorsStore from './Errors'
import * as SegmentStore from './Segments'
import * as ProductTypeStore from './ProductTypes'
import * as ProductVariantStore from './ProductVariants'
import * as ProductMatrixStore from './ProductMatrix'
import * as TopicStore from './Topics';
import * as CustomAttributeValueStore from './CustomAttributeValue';
import * as ProductVariantDocuments from './ProductVariantDocuments';
import * as CustomAttributesStore from './CustomAttributes';
import * as ProfileStore from './Profile';
import * as superagent from 'superagent';
import { reducer as oidcReducer, UserState } from 'redux-oidc';
import { activeRequests } from '../components/common/LoaderWrapper';
import { reducer as formReducer } from 'redux-form';
import { reducer as toastrReducer } from 'react-redux-toastr'

// The top-level state object
export interface ApplicationState {
    codebooks: CodebooksStore.CodebooksState,
    errors: ErrorsStore.ErrorState,
    segments: SegmentStore.SegmentsState,
    productTypes: ProductTypeStore.ProductTypesState,
    productVariants: ProductVariantStore.ProductVariantsState,
    productMatrix: ProductMatrixStore.ProductMatrixState
    topics: TopicStore.TopicState,
    productVariantDocuments: ProductVariantDocuments.ProductVariantDocumentsState,
    oidcReducer: UserState,
    customAttributeValue: CustomAttributeValueStore.CustomAttributeValueState,
    customAttributes: CustomAttributesStore.CustomAttributeState,
    profile: ProfileStore.ProfileState
}

// Whenever an action is dispatched, Redux will update each top-level application state property using
// the reducer with the matching name. It's important that the names match exactly, and that the reducer
// acts on the corresponding ApplicationState property type.
export const reducers = {
    codebooks: CodebooksStore.reducer,
    errors: ErrorsStore.reducer,
    segments: SegmentStore.reducer,
    productTypes: ProductTypeStore.reducer,
    productVariants: ProductVariantStore.reducer,
    productMatrix: ProductMatrixStore.reducer,
    topics: TopicStore.reducer,
    oidcReducer,
    activeRequests,
    form: formReducer,
    toastr: toastrReducer,
    customAttributeValue: CustomAttributeValueStore.reducer,
    productVariantDocuments: ProductVariantDocuments.reducer,
    customAttributes: CustomAttributesStore.reducer,
    profile: ProfileStore.reducer
};

// This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// correctly typed to match your store.
export interface AppThunkAction<TAction> {
    (dispatch: (action: TAction) => void,
        getState: () => ApplicationState,
        superagent: superagent.SuperAgentStatic,
        responseHandler: (
            validCallback: (response: superagent.Response) => void,
            invalidCallback?: (response: superagent.Response) => void,
            serverErrorCallback?: (response: superagent.Response) => void) => (response: superagent.Response) => Promise<superagent.Response>)
}