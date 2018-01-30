import { AppThunkAction } from './';
import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { MeasureUnitVM } from '../apimodels/Codebooks';

export interface CodebooksState {
    measureUnits: MeasureUnitVM[]
}

export enum CodebooksActionTypes {
    RequestMeasureUnits = 'REQUEST_MEASUREUNITS',
    ReceiveMeasureUnits = 'RECEIVE_MEASUREUNITS'
}

interface RequestMeasureUnits {
    type: CodebooksActionTypes.RequestMeasureUnits;
}

interface ReceiveMeasureUnits {
    type: CodebooksActionTypes.ReceiveMeasureUnits;
    measureUnits: MeasureUnitVM[];
}

type CodebooksAction =
    RequestMeasureUnits
    | ReceiveMeasureUnits

export const actionCreators = {
    requestMeasureUnits: (): AppThunkAction<CodebooksAction> => async (dispatch, getState, superagent, responseHandler) => {
        dispatch({ type: CodebooksActionTypes.RequestMeasureUnits });

        await superagent.get('/api/codebooks/measureunits')
            .then(responseHandler(
                response => dispatch({ type: CodebooksActionTypes.ReceiveMeasureUnits, measureUnits: response.body })
            ))
            .catch(reason => console.warn(reason));
    }
}

const unloadedState = { measureUnits: [] }

export const reducer: Reducer<CodebooksState> = (state: CodebooksState, action: CodebooksAction) => {
    switch (action.type) {
        case CodebooksActionTypes.RequestMeasureUnits:
            return {...state};
        case CodebooksActionTypes.ReceiveMeasureUnits:
            return {
                measureUnits: action.measureUnits
            }
        default:
            // The following line guarantees that every action in union has been covered by a case above
            const exhaustiveCheck: never = action;
    }
    return state || unloadedState
}