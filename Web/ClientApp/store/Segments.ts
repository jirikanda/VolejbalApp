import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import config from '../configuration/config';
import { SegmentVM } from '../apimodels/SegmentsStore';

export interface SegmentsState {
    segments: SegmentVM[];
    segment: SegmentVM;
}

export enum SegmentsActionTypes {
    RequestSegments = 'REQUEST_SEGMENTS',
    ReceiveSegments = 'RECEIVE_SEGMENTS',
    RequestSegmentById = 'REQUEST_SEGMENT_BY_ID',
    ReceiveSegmentById = 'RECEIVE_SEGMENT_BY_ID',
    SelectSegment = 'SELECT_SEGMENT'
}

interface RequestSegmentsAction {
    type: SegmentsActionTypes.RequestSegments
}

interface ReceiveSegmentsAction {
    type: SegmentsActionTypes.ReceiveSegments
    segments: SegmentVM[]
}

interface RequestSegmentByIdAction {
    type: SegmentsActionTypes.RequestSegmentById
}

interface ReceiveSegmentByIdAction {
    type: SegmentsActionTypes.ReceiveSegmentById,
    segment: SegmentVM;
}

interface SelectSegmentAction {
    type: SegmentsActionTypes.SelectSegment,
    segment: SegmentVM;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type SegmentsAction = RequestSegmentsAction | ReceiveSegmentsAction | RequestSegmentByIdAction | ReceiveSegmentByIdAction | SelectSegmentAction;

export const actionCreators = {
    requestSegments: (): AppThunkAction<SegmentsAction> => (dispatch, getState, superagent, responseHandler) => {
        const request = superagent.get('/api/segmentsstore/segments')
            .then(responseHandler(
                response => dispatch({ type: SegmentsActionTypes.ReceiveSegments, segments: response.body })))
            .catch(reason => console.warn(reason));

        addTask(request); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: SegmentsActionTypes.RequestSegments });
    },
    requestSegmentById: (segmentId: number): AppThunkAction<SegmentsAction> => (dispatch, getState, superagent, responseHandler) => {
        const request = superagent
            .get(`/api/segmentsstore/segments/${segmentId}`)
            .then(responseHandler(
                response => dispatch({ type: SegmentsActionTypes.ReceiveSegmentById, segment: response.body })))
            .catch(reason => console.warn(reason));

        addTask(request); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: SegmentsActionTypes.RequestSegmentById });
    },
    selectSegment: (segment: SegmentVM): AppThunkAction<SegmentsAction> => (dispatch, getState, superagent, responseHandler) => {
        dispatch({ type: SegmentsActionTypes.SelectSegment, segment })
    }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: SegmentsState = { segments: [], segment: null };

export const reducer: Reducer<SegmentsState> = (state: SegmentsState, action: SegmentsAction) => {
    switch (action.type) {
        case SegmentsActionTypes.RequestSegments:
            return {
                segments: state.segments,
                segment: state.segment
            };
        case SegmentsActionTypes.ReceiveSegments:
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            return {
                segments: action.segments,
                segment: state.segment
            };
        case SegmentsActionTypes.RequestSegmentById:
            return {
                segments: state.segments,
                segment: state.segment
            };
        case SegmentsActionTypes.ReceiveSegmentById:
        case SegmentsActionTypes.SelectSegment:
            return {
                segments: state.segments,
                segment: action.segment
            }
        default:
            // The following line guarantees that every action in the SegmentsAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState;
};
