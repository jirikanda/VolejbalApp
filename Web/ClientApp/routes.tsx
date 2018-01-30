import * as React from 'react';
import { Route, Switch } from 'react-router-dom';
import Layout from './components/Layout';
import Home from './containers/Home';
import Administration from './components/Administration';
import Segments from './containers/segments/Segments';
import ProductConfiguration from './containers/pages/products/ProductConfiguration';
import Login from './containers/pages/Login';
import CallbackPage from './containers/pages/CallbackPage';
import loaderWrapperFactory from './components/common/LoaderWrapper';
import AuthenticatedUser from './containers/AuthenticatedUser'
import { Logout } from './containers/pages/Logout'
import ErrorBoundary from './containers/ErrorBoundary';
import ProductMatrix from './containers/products/ProductMatrix';
import TopicsCustomAttributesEdit from './containers/pages/administration/TopicsCustomAttributesEdit';

import * as CodebooksStore from './store/Codebooks'
import * as SegmentStore from './store/Segments'
import * as ProductTypeStore from './store/ProductTypes'
import * as ErrorsStore from './store/Errors'
import * as ProductMatrixStore from './store/ProductMatrix'
import * as ProductVariantsStore from './store/ProductVariants'
import * as CustomAttributesStore from './store/CustomAttributes'
import * as ProfileStore from './store/Profile'

const actionCreatorsCombined = {
    ...CodebooksStore.actionCreators,
    ...SegmentStore.actionCreators,
    ...ProductTypeStore.actionCreators,
    ...ErrorsStore.actionCreators,
    ...ProductMatrixStore.actionCreators,
    ...ProductVariantsStore.actionCreators,
    ...CustomAttributesStore.actionCreators,
    ...ProfileStore.actionCreators
}

const actionTypesCombined = {
    ...CodebooksStore.CodebooksActionTypes,
    ...SegmentStore.SegmentsActionTypes,
    ...ProductTypeStore.ProductTypesActionTypes,
    ...ErrorsStore.ErrorsActionTypes,
    ...ProductMatrixStore.ProductMatrixActionTypes,
    ...ProductVariantsStore.ProductVariantsActionTypes,
    ...CustomAttributesStore.CustomAttributeActionTypes,
    ...ProfileStore.ProfileActionTypes
}

const actionCreators = Object.keys(actionCreatorsCombined).map(key => actionCreatorsCombined[key]);
const actionTypes = Object.keys(actionTypesCombined).map(key => actionTypesCombined[key]);

const loaderWrapper = loaderWrapperFactory(actionCreators, actionTypes, (state) => ({ activeRequests: state.activeRequests }));

const LayoutWithLoader = loaderWrapper(Layout);

export const routes =
    <LayoutWithLoader>
        <Switch>
            <Route exact path='/' component={Home} />
            <Route exact path='/callback' component={CallbackPage} />
            <Route exact path='/logout' component={Logout}></Route>
            <AuthenticatedUser>
                <Route exact path='/segments' component={Segments} />
                <Route exact path='/segments/:segmentId/product-types/:productTypeId' component={ProductConfiguration} />
                <Route exact path='/segments/:segmentId/product-types/:productTypeId/product-matrix' component={ProductMatrix} />
                <Route exact path='/segments/administration' component={Administration} />
                <Route exact path='/segments/administration/topics-custom-attributes-edit' component={TopicsCustomAttributesEdit} />                
            </AuthenticatedUser>            
        </Switch>
    </LayoutWithLoader>