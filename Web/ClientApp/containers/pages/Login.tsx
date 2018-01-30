import * as React from 'react';
import userManager from './../../oidc/userManager';
import { UserState } from 'redux-oidc';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import { Row, Col, Card, CardBody, CardTitle, Button } from 'reactstrap';
import { push, replace } from 'react-router-redux';

type LoginProps = UserState

class Login extends React.Component<LoginProps> {
    onLoginButtonClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        if (/*!this.props.user || this.props.user.expired*/ true) {
            userManager.signinRedirect({
                data: {
                    redirectUrl: window.location.href.split('/').pop()
                }
            });
        }
    };

    render() {
        return <div className="container">
            <Button color="primary" onClick={this.onLoginButtonClick}>Přihlásit</Button>
        </div>;
    }
}

export default connect((state: ApplicationState) => state.oidcReducer)(Login);