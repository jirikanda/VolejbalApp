import * as superagent from 'superagent';
import config from '../configuration/config';
import userManager from '../oidc/userManager';
import * as HttpStatusCodes from 'http-status-codes';
import { Toastr } from '../store/commonActionCreators/Toaster';
import { actionCreators } from '../store/Errors';
import { ErrorsAction, ErrorsActionTypes } from '../store/Errors';
import { ValidationError } from '../apimodels/ValidationErrors';

const superagentUse = require('superagent-use')
const superagentInstance = superagentUse(require('superagent'));
const superagentUsePrefix = require('superagent-prefix');

// prepend prefix (host) to every superagent request
superagentInstance.use(superagentUsePrefix(config.apiSettings.webApiUrl));

const createSuperagentInstance = () => superagentInstance as superagent.SuperAgentStatic;

const defaultErrorCallback = (response: superagent.Response) => {
    const error = response.body as ValidationError;
    if (error.message) {
        Toastr.error({ message: error.message })
    } else {
        error.errors.forEach(err => Toastr.error({ message: err.message }))        
    }    
}

export const responseHandler = (dispatch: (action: ErrorsAction) => void) => (
    validCallback: (response: superagent.Response) => void,
    invalidCallback?: (response: superagent.Response) => void,
    serverErrorCallback?: (response: superagent.Response) => void) => (response: superagent.Response) => {
    switch (response.status) {
        case HttpStatusCodes.OK:
            validCallback(response);
            break;
        case HttpStatusCodes.UNPROCESSABLE_ENTITY:
            invalidCallback ? invalidCallback(response) : defaultErrorCallback(response);
            break;
        case HttpStatusCodes.UNAUTHORIZED:
        case HttpStatusCodes.FORBIDDEN:
        case HttpStatusCodes.NOT_FOUND:
        case HttpStatusCodes.METHOD_NOT_ALLOWED:
            dispatch({ type: ErrorsActionTypes.ErrorApplication, applicationError: { message: response.body.message } })
            break;
        case HttpStatusCodes.INTERNAL_SERVER_ERROR:
        default:
            serverErrorCallback ? serverErrorCallback(response) : defaultErrorCallback(response);
            //dispatch({ type: ErrorsActionTypes.ErrorApplication, applicationError: { message: null } })
            break;
        }
        return Promise.resolve(response);
}

export default createSuperagentInstance