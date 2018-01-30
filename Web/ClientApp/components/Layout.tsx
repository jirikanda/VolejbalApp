import * as React from "react";
import { Breadcrumb, BreadcrumbItem, Button } from "reactstrap";
import { withRouter, RouteComponentProps, matchPath } from 'react-router';
import { Route, Link, NavLink } from 'react-router-dom';
import AuthenticatedUser from '../containers/AuthenticatedUser'
import BreadCrumbs from '../containers/BreadCrumbs';
import ErrorBoundary from '../containers/ErrorBoundary';
import Menu from '../containers/Menu';
import ExportToPDFButton from '../containers/products/ExportToPDFButton';
import GoToConfigurationPageButton from '../containers/products/GoToConfigurationPageButton';
import ProductTypeName from '../containers/products/ProductTypeName';
import SegmentName from '../containers/segments/SegmentName';
import SelectedTopicsSaveButtonContainer from '../containers/topics/SelectedTopicsSaveButtonContainer';
import { ApplicationState } from '../store/index';
import ScrollHandler from './common/ScrollHandler';
import { connect } from "react-redux";

const breadCrumbMap = [
    { path: '/', renderFactory: () => () => '' },
    { path: '/segments', renderFactory: () => () => 'Metodiky online' },
    { path: '/segments/:segmentId/product-types/:productTypeId', renderFactory: ({ segmentId }) => () => <SegmentName segmentId={segmentId} /> },
    { path: '/segments/:segmentId/product-types/:productTypeId/product-matrix', renderFactory: ({ productTypeId }) => () => <ProductTypeName productTypeId={productTypeId} /> },
    { path: '/segments/administration', renderFactory: () => () => 'Administrace' },
    { path: '/segments/administration/topics-custom-attributes-edit', renderFactory: () => () => 'Editace témat a upřesňujících podmínek' }
];

type LayoutProps =
    RouteComponentProps<{ productTypeId: number; segmentId: number }>
    & { isQuickSelection: boolean }

class Layout extends React.Component<LayoutProps> {

    handleScroll = (event: UIEvent, element: Element) => {
        if (location.href.indexOf("product-matrix") > -1) {
            if ((element.scrollHeight < window.scrollY) && !element.classList.contains("fixed-top")) {
                element.classList.add("fixed-top");
            }

            if ((element.scrollHeight > window.scrollY) && element.classList.contains("fixed-top")) {
                element.classList.remove("fixed-top")
            }
        }
    }

    render() {
        const { isQuickSelection, children } = this.props;

        return <>
            <Menu {...this.props} />
            <ScrollHandler onScroll={this.handleScroll}>
                <Breadcrumb>
                    <BreadcrumbItem>Home</BreadcrumbItem>
                    <AuthenticatedUser>
                        <BreadCrumbs {...this.props} breadCrumbMap={breadCrumbMap} />
                        <div className="ml-auto float-right">
                            <SelectedTopicsSaveButtonContainer isVisible={location.pathname.includes("product-matrix") && !isQuickSelection} />
                            <GoToConfigurationPageButton {...this.props}  isVisible={location.pathname.includes("product-matrix") && isQuickSelection} />
                            {" "}
                            {location.pathname.includes("product-matrix") && <ExportToPDFButton />}
                        </div>
                    </AuthenticatedUser>
                </Breadcrumb>
            </ScrollHandler>
            <ErrorBoundary {...this.props}>
                {children}
            </ErrorBoundary>
        </>
    }
}

export default withRouter(connect((state: ApplicationState) => ({
    isQuickSelection: state.productMatrix.isQuickSelection
}))(Layout))