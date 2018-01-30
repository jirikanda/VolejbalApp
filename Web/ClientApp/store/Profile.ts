import { AppThunkAction } from './';
import { Action, Reducer, ActionCreator } from 'redux';
import { ProfileVM } from '../apimodels/Profile';

export interface ProfileState {
    profile: ProfileVM
}

export enum ProfileActionTypes {
    RequestProfile = 'REQUEST_PROFILE',
    ReceiveProfile = 'RECEIVE_PROFILE'
}

export interface RequestProfile {
    type: ProfileActionTypes.RequestProfile;
}

export interface ReceiveProfile {
    type: ProfileActionTypes.ReceiveProfile;
    profile: ProfileVM;
}

type ProfileAction = RequestProfile | ReceiveProfile

export const actionCreators = {
    requestProfile: (): AppThunkAction<ProfileAction> => (dispatch, getState, superagent, responseHandler) => {
        dispatch({ type: ProfileActionTypes.RequestProfile });

        superagent.get('/api/profile')
            .then(responseHandler(
                response => dispatch({ type: ProfileActionTypes.ReceiveProfile, profile: response.body })
            )).catch(reason => console.warn(reason));
    }
}

const unloadedState = { profile: null }

export const reducer: Reducer<ProfileState> = (state: ProfileState, action: ProfileAction) => {
    switch (action.type) {
        case ProfileActionTypes.RequestProfile:
            return { ...state };
        case ProfileActionTypes.ReceiveProfile:
            return {
                profile: action.profile
            }
        default:
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState
}