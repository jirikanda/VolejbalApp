import * as React from 'react';
import userManager from '../oidc/userManager';
import { push, replace } from 'react-router-redux';
import { Redirect, withRouter, RouteComponentProps } from 'react-router';
import { connect } from 'react-redux';
import { ApplicationState } from 'ClientApp/store';
import { UserState } from 'redux-oidc';

type LogoutProps = UserState

class Logout extends React.Component<LogoutProps> {
    async componentWillMount() {
        if (this.props.user) {
            await userManager.revokeAccessToken();
            await userManager.removeUser();
            await userManager.signoutRedirect();
        }
    }

    render() {
        return this.props.user == undefined && <Redirect to='/' />;
    }
}

export default connect((state: ApplicationState) => state.oidcReducer)(Logout)