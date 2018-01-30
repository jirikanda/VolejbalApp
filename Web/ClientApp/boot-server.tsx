import * as React from 'react';
import { Provider } from 'react-redux';
import { renderToString } from 'react-dom/server';
import { StaticRouter } from 'react-router-dom';
import { replace } from 'react-router-redux';
import { createMemoryHistory } from 'history';
import { createServerRenderer, RenderResult } from 'aspnet-prerendering';
import { routes } from './routes';
import configureStore from './configureStore';
import config from './configuration/config';
import * as moment from 'moment';

import createSuperagent from './apiclient/superagentWrapper';
const superagentInstance = createSuperagent();

moment.locale("cs");

export default createServerRenderer(params => {
    return new Promise<RenderResult>((resolve, reject) => {
        // Prepare Redux store with in-memory history, and dispatch a navigation event
        // corresponding to the incoming URL
        const store = configureStore(createMemoryHistory(), superagentInstance);
        store.dispatch(replace(params.location));

        // Prepare an instance of the application and perform an inital render that will
        // cause any async tasks (e.g., data access) to begin
        const routerContext: { url: string } = { url: null };
        const app = (
            <Provider store={store}>
                <StaticRouter context={routerContext} location={params.location.path} children={routes} />
            </Provider>
        );
        renderToString(app);

        // If there's a redirection, just send this information back to the host application
        if (routerContext.url) {
            resolve({ redirectUrl: routerContext.url });
            return;
        }

        // Once any async tasks are done, we can perform the final render
        // We also send the redux store state, so the client can continue execution where the server left off
        params.domainTasks.then(() => {
            resolve({
                html: renderToString(app),
                globals: {
                    initialReduxState: store.getState(),
                    prerenderedData: params.data
                }
            });
        }, reject); // Also propagate any errors back into the host application
    });
});
