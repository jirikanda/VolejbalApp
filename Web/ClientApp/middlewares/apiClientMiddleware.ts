import * as superagent from 'superagent';
import { ApplicationState } from '../store/index';
import { Toastr } from '../store/commonActionCreators/Toaster';
import { responseHandler } from '../apiclient/superagentWrapper';

const setAuthorizationHeader = (user: Oidc.User) => {
    return (request: superagent.Request) => {
        if (user) {
            request.set("Authorization", `Bearer ${user.id_token}`);
        }
        return request;
    };
}

// every response is valid - handling is in superagentWrapper in responseHanlder()
const setAllResponseAreValid = () => (request: superagent.Request) => request.ok(response => true)

const apiClientMiddleware = (apiClient) => {
    return ({ dispatch, getState }) => {
        apiClient.use(setAllResponseAreValid());
        return next => action => {
            if (typeof action === 'function') {
                const user = getState().oidcReducer.user;
                if (user) {
                    apiClient.use(setAuthorizationHeader(user));
                }

                return action(dispatch, getState, apiClient, responseHandler(dispatch))
            } else {
                return next(action)
            }
        }
    }
}

export default apiClientMiddleware;