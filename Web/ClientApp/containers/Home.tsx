import * as React from 'react';
import { connect } from 'react-redux';
import userManager from "../oidc/userManager";
import { ApplicationState } from '../store/index';
import { Redirect, withRouter, RouteComponentProps } from 'react-router';

interface HomeProps {
    user: Oidc.User
}

class Home extends React.Component<HomeProps & RouteComponentProps<Object>> {
    componentWillMount() {
        if (this.props.user == undefined) {
            userManager.signinRedirect({
                data: {
                    redirectUrl: window.location.href.split('/').pop()
                }
            });
        }
    }
    render() {
        if (this.props.user) {
            return <Redirect to='/segments' />
        }
        return null;
    }
}

export default withRouter(connect((state: ApplicationState) => state.oidcReducer, null)(Home));