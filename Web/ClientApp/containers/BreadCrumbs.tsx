import * as React from "react";
import { Route, Link, NavLink } from 'react-router-dom';
import { Breadcrumb, BreadcrumbItem, Button } from "reactstrap";
import { withRouter, RouteComponentProps, matchPath } from 'react-router';
import { Location } from 'history';

export interface BreadCrumbMapItem {
    path: string;
    renderFactory: (...args) => () => string | JSX.Element;
}

type BreadcrumbsProps = RouteComponentProps<{}> & { breadCrumbMap: BreadCrumbMapItem[] }

const getBreadcrumbs = (location: Location, breadCrumbMap: BreadCrumbMapItem[]) => {
    const matches = [];

    location.pathname.split('/')
        .reduce((previous, current) => {
            const pathSection = `${previous}/${current}`;
            let match;
            const breadCrumb = breadCrumbMap.find(breadCrumbMapItem => {
                match = matchPath(pathSection, { exact: true, path: breadCrumbMapItem.path })
                return match;
            });

            if (breadCrumb) {
                matches.push({
                    component: <Route {...match } render={breadCrumb.renderFactory(match.params)} />,
                    path: pathSection
                });
            }

            return pathSection;
        });

    return matches;
};

class Breadcrumbs extends React.Component<BreadcrumbsProps> {
    render() {
        const { location, breadCrumbMap } = this.props;

        const breadcrumbs = getBreadcrumbs(location, breadCrumbMap);

        return breadcrumbs.map(({ component, path }, index) => (
            <BreadcrumbItem key={path}>
                {((breadcrumbs.length - 1) === index) ? component : <NavLink to={path}>{component}</NavLink>}
            </BreadcrumbItem>
        ));
    }    
};

export default withRouter(Breadcrumbs)