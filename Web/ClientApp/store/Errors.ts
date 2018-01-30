import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import config from '../configuration/config';
import { ApplicationError as ApplicationErrorModel } from '../models/ApplicationError';

export interface ErrorState {
    pathname?: string;
    error?: Error;
    errorInfo?: React.ErrorInfo;
    applicationError?: ApplicationErrorModel;
}

export enum ErrorsActionTypes {
    ErrorTrack = 'ERROR_TRACK',
    ErrorClear = 'ERROR_CLEAR',
    ErrorApplication = 'ERROR_APPLICATION'
}

interface ErrorTrack {
    type: ErrorsActionTypes.ErrorTrack;
    pathname: string;
    error: Error;
    errorInfo?: React.ErrorInfo;
}

interface ErrorClear {
    type: ErrorsActionTypes.ErrorClear;
}

interface ErrorApplication {
    type: ErrorsActionTypes.ErrorApplication;
    applicationError: ApplicationErrorModel;
    pathname?: string;
}

export type ErrorsAction = ErrorTrack | ErrorClear | ErrorApplication;

export const actionCreators = {
    trackError: (pathname: string, error: Error, errorInfo?: React.ErrorInfo): AppThunkAction<ErrorsAction> => (dispatch, getState) => dispatch({ type: ErrorsActionTypes.ErrorTrack, pathname, error, errorInfo }),
    clearError: (): AppThunkAction<ErrorsAction> => (dispatch, getState) => dispatch({ type: ErrorsActionTypes.ErrorClear })
}

const unloadedState: ErrorState = { pathname: null, error: null, errorInfo: null, applicationError: null };

export const reducer: Reducer<ErrorState> = (state: ErrorState, action: ErrorsAction) => {
    switch (action.type) {
        case ErrorsActionTypes.ErrorTrack:
            return {
                pathname: action.pathname,
                error: action.error,
                errorInfo: action.errorInfo,
                applicationError: state.applicationError
            }
        case ErrorsActionTypes.ErrorApplication:
            return {
                pathname: action.pathname,
                error: state.error,
                errorInfo: state.errorInfo,
                applicationError: action.applicationError
            }
        case ErrorsActionTypes.ErrorClear:
            return unloadedState;
        default:
            const exhaustiveCheck: never = action;        
    }

    return state || unloadedState;
}