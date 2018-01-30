import * as React from 'react';
import { connect, Dispatch } from 'react-redux';
import { CallbackComponent, UserState } from 'redux-oidc';
import { push } from 'react-router-redux';
import userManager from './../../oidc/userManager';
import { User } from 'oidc-client';
import { ApplicationState } from '../../store/';
import * as CodebooksStore from '../../store/Codebooks';
import * as ProfileStore from '../../store/Profile';
import { bindActionCreators } from 'redux';

interface CallbackPageProps {
    dispatch: Dispatch<any>;
}

type CallbackPagePropsPublic = UserState
    & CallbackPageProps
    & typeof CodebooksStore.actionCreators
    & typeof ProfileStore.actionCreators

class CallbackPage extends React.Component<CallbackPagePropsPublic> {
    successCallback = (user: User) => {
        this.props.requestProfile();
        this.props.requestMeasureUnits();
        this.props.dispatch(push('/'));        
    }

    errorCallback = (error: Error) => {
        console.error(error);
    }

    render() {
        return (
            <CallbackComponent userManager={userManager} successCallback={this.successCallback} errorCallback={this.errorCallback}>
                <div></div>
            </CallbackComponent>
        );
    }
}

const mapDispatchToProps = (dispatch, ownProps) => {
    return {
        ...{ dispatch },
        ...bindActionCreators(CodebooksStore.actionCreators, dispatch),
        ...bindActionCreators(ProfileStore.actionCreators, dispatch)
    }
};

export default connect((state: ApplicationState) => state.oidcReducer, mapDispatchToProps)(CallbackPage);