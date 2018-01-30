import * as React from 'react';
import { NavLink, Link, withRouter, RouteComponentProps } from 'react-router-dom';
const logo = require('./../src/img/btm_logo.png');

type AdministrationProps =
    RouteComponentProps<{}>;

class Administration extends React.Component<AdministrationProps> {

    onAdministrationDashboardItemClick = (redirectTo: string) => this.props.history.push(redirectTo)

    render() {
        return <div className='container administration-page'>
            {/* <div className='row'>
                <h2><i className="fa fa-cogs" aria-hidden="true"></i> Dashboard</h2>
            </div> */}
            <div className='row'>
                <div className='container-fluid dashboard'>
                    <div className="row dashboard-header dashboard-blue-bg">
                        <h5 className=''>Section 1</h5>
                    </div>
                    <div className="row dashboard-body">
                        <div className="dashboard-item dashboard-blue-border" onClick={e => this.onAdministrationDashboardItemClick("/segments/administration/topics-custom-attributes-edit")}>
                            <div><i className="fa fa-sitemap" aria-hidden="true"></i></div>
                            Editace témat a upřesňujících podmínek
                        </div>
                        <div className="dashboard-item dashboard-blue-border">
                            <div><i className="fa fa-address-card-o" aria-hidden="true"></i></div>
                            Function B
                        </div>
                        <div className="dashboard-item dashboard-blue-border">
                            <div><i className="fa fa-file-text-o" aria-hidden="true"></i></div>
                            Function C
                        </div>
                    </div>
                </div>
            </div>
            <br />
            <div className='row'>
                <div className='container-fluid dashboard'>
                    <div className="row dashboard-header dashboard-green-bg">
                        <h5 className=''>Section 2</h5>
                    </div>
                    <div className="row dashboard-body">
                        <div className="dashboard-item dashboard-green-border">
                            <div><i className="fa fa-users" aria-hidden="true"></i></div>
                            Function A
                        </div>
                        <div className="dashboard-item dashboard-green-border">
                            <div><i className="fa fa-pie-chart" aria-hidden="true"></i></div>
                            Function B
                        </div>
                        <div className="dashboard-item dashboard-green-border">
                            <div><i className="fa fa-table" aria-hidden="true"></i></div>
                            Function C
                        </div>
                        <div className="dashboard-item dashboard-green-border">
                            <div><i className="fa fa-star-half-o" aria-hidden="true"></i></div>
                            Function D
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
}

export default withRouter(Administration);