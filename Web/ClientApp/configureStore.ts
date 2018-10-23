import { createStore, applyMiddleware, compose, combineReducers, StoreEnhancer, Store } from 'redux';
import * as StoreModule from './store';
import createSagaMiddleware from 'redux-saga'
import { routerMiddleware, connectRouter } from 'connected-react-router'
import rootSaga from './store/root-saga';
import { History } from 'history'

const configureStore = (history: History, initialState?: StoreModule.ApplicationState) => {
    const rootReducer = buildRootReducer(StoreModule.reducers);
    const sagaMiddleware = createSagaMiddleware();
    const composeEnhancer: typeof compose = (window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose
    const store = createStore(
        connectRouter(history)(rootReducer),
        initialState,
        composeEnhancer(
            applyMiddleware(
                routerMiddleware(history),
                sagaMiddleware,
                )
        )
    )

    sagaMiddleware.run(rootSaga);
    
    // Enable Webpack hot module replacement for reducers
    if (module.hot) {
        module.hot.accept('./store', () => {
            const nextRootReducer = require<typeof StoreModule>('./store');
            store.replaceReducer((connectRouter(history)(buildRootReducer(nextRootReducer.reducers))));
        });
    }

    return store;
}

const buildRootReducer = (allReducers) => {
    return combineReducers<StoreModule.ApplicationState>({ ...allReducers });
}

export default configureStore