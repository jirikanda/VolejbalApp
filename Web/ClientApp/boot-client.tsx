require('es6-shim');

import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { AppContainer } from 'react-hot-loader';
import { Provider } from 'react-redux';
import { ConnectedRouter } from 'react-router-redux';
import { createBrowserHistory } from 'history';
import configureStore from './configureStore';
import { ApplicationState } from './store';
import config from './configuration/config';
import * as moment from 'moment';
import { OidcProvider } from 'redux-oidc';
import userManager from './oidc/userManager';
import { loadState, saveState } from './store/storage/ReduxStateStorage';
import ReduxToastr from 'react-redux-toastr';
import HTML5Backend from 'react-dnd-html5-backend';
import { DragDropContextProvider } from 'react-dnd';
import { Route } from 'react-router';

import "./src/css/style.scss";

import * as RoutesModule from './routes';
let routes = RoutesModule.routes;

import createSuperagent from './apiclient/superagentWrapper';
const superagentInstance = createSuperagent();

moment.locale("cs");

// Create browser history to use in the Redux store
const history = createBrowserHistory();

// Get the application-wide store instance, prepopulating with state from the server where available.
let initialState = window["initialReduxState"] as ApplicationState;

// Get application state from storage
const persistedState = loadState();

if (persistedState && initialState === undefined) {
    initialState = persistedState;
}

const store = configureStore(history, superagentInstance, initialState);

// Save whole or partial application state to storage
// PK: disabled because of large data exceeded the browser storage quota
// LR: codebooks definitely needed
 store.subscribe(() => {
     saveState({
         codebooks: store.getState().codebooks
     });
 })

const renderApp = () => {
    // This code starts up the React app when it runs in a browser. It sets up the routing configuration
    // and injects the app into a DOM element.
    ReactDOM.render(
        <AppContainer>
            <DragDropContextProvider backend={HTML5Backend}>
                <Provider store={store}>
                    <div>
                        <OidcProvider store={store} userManager={userManager}>
                            <ConnectedRouter history={history}>
                                <Route>
                                    {routes}
                                </Route>
                            </ConnectedRouter>
                        </OidcProvider>
                        <ReduxToastr
                            newestOnTop={true}
                            preventDuplicates
                            position="top-right"
                            transitionIn="fadeIn"
                            transitionOut="fadeOut" />
                    </div>
                </Provider>
            </DragDropContextProvider>
        </AppContainer>,
        document.getElementById('app')
    );
}

renderApp();

// Allow Hot Module Replacement
if (module.hot) {
    module.hot.accept('./routes', () => {
        routes = require<typeof RoutesModule>('./routes').routes;
        renderApp();
    });
}
