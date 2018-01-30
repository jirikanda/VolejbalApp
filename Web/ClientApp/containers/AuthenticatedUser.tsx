import * as React from 'react';
import userManager from './../oidc/userManager';
import { UserState, } from 'redux-oidc';
import { connect } from 'react-redux';
import { ApplicationState } from '../store/index';
import { push, replace } from 'react-router-redux';
import { Redirect } from 'react-router';
import Login from './pages/Login';

type AuthenticatedUserProps = UserState

class AuthenticatedUser extends React.Component<AuthenticatedUserProps> {
    render() {
        if (this.props.user && !this.props.isLoadingUser) {
            return this.props.children;
        } else {
            return null;
        }
    }
}

export default connect((state: ApplicationState) => state.oidcReducer)(AuthenticatedUser);