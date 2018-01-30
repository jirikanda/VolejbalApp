import * as React from "react";
import { NavLink, Link } from "react-router-dom";
import userManager from "../oidc/userManager";
import * as ProfileStore from '../store/Profile';
import {
    Collapse,
    Navbar,
    NavbarToggler,
    NavbarBrand,
    Nav,
    NavItem
} from "reactstrap";
import { ProfileVM } from "../apimodels/Profile";

const logo = require("./../src/img/btm_logo.png");

interface MenuComponentProps {
    user: Oidc.User;
    profile: ProfileVM;
}

export class MenuComponent extends React.PureComponent<MenuComponentProps> {
    render() {
        const { user, profile } = this.props;

        let navbarLogo = <NavLink exact to={"/"}>
            <img
                src={logo}
                className="navbar-brand"
                alt="Broker Trust Metodiky"
            />
        </NavLink>;

        let navbarStyle;
        let navbarContent;

        if (user && profile) {
            const currentProfile = profile.partners.find(partner => partner.partnerId === profile.partnerId);

            navbarLogo = <div className="page-logo"><NavLink exact to={"/"}>
                <img src={currentProfile && currentProfile.branding.logoUrl} className="navbar-brand page-logo" alt="Broker Trust Metodiky" />
            </NavLink></div>

            navbarStyle = {
                backgroundImage: `linear-gradient(to bottom, #${currentProfile.branding.headerGradientTopColor}, #${currentProfile.branding.headerGradientBottomColor})`
            }

            navbarContent =
                <ul className="nav navbar-nav">
                    <li className="nav-item">
                        <a className="nav-link" href="javascript:void(0)">
                            Přihlášený: <strong>{profile.loginAccountName} ({profile.username})</strong>
                            {currentProfile && " jako "}
                            {currentProfile && <strong>{currentProfile.partnerName} ({currentProfile.partnerCode})</strong>}
                        </a>
                    </li>
                    <li className="nav-item">
                        <a className="nav-link" href="javascript:void(0)">
                            <i className="fa fa-eye fa-lg" aria-hidden="true"></i>
                        </a>
                    </li>
                    <li className="nav-item">
                        <NavLink className="nav-link" exact to={"/segments/administration"}>
                            <i className="fa fa-cog fa-lg" aria-hidden="true"></i>
                        </NavLink>
                    </li>
                    <li className="nav-item">
                        <NavLink className="nav-link" exact to={"/logout"}>
                            <i className="fa fa-power-off fa-lg" aria-hidden="true"></i>
                        </NavLink>
                    </li>
                </ul>;
        }

        return (
            <nav className="navbar navbar-expand-lg bg-navbar" style={navbarStyle}>
                <div className="navbar-nav">
                    {navbarLogo}                    
                </div>
                <div id="navbarNavDropdown" className="navbar-collapse">
                    <div className="mr-auto"></div>
                    {navbarContent}
                </div>
            </nav>
        );
    }
}
