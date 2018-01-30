import * as React from "react";
import { NavLink, RouteComponentProps, withRouter } from "react-router-dom";
import { connect } from "react-redux";
import * as ProductTypeStore from "../../../store/ProductTypes";
import * as ProductVariantsStore from "../../../store/ProductVariants";
import { compose } from "redux";
import { ApplicationState } from "../../../store";
import ProductTypes from "../../products/ProductTypes";
import ProductVariants from "../../../containers/products/ProductVariants";
import { Card, CardBody, CardTitle, CardHeader, TabContent, Container, Button } from "reactstrap";
import SegmentName from "../../segments/SegmentName";
import ToggleAllTopics from "../../topics/ToggleAllTopics";
import ToggleAllProducts from "../../products/ToggleAllProducts";
import SelectedTopics from "../../topics/SelectedTopics";
import ShowProductMatrix from "../../products/ShowProductMatrix";
import ShowProductMatrixLimitedSelection from "../../products/ShowProductMatrixLimitedSelection";
import SelectedTopicsLoadButtonContainer from "../../topics/SelectedTopicsLoadButtonContainer";
import SelectedTopicsSaveButtonContainer from "../../topics/SelectedTopicsSaveButtonContainer";
import SelectedProducersSaveButtonContainer from "../../../containers/products/SelectedProducersSaveButtonContainer";
import SelectedProducersLoadButtonContainer from "../../../containers/products/SelectedProducersLoadButtonContainer";
import TopicSearchContainer from "../../topics/TopicSearchContainer";

type ProductTypesProps = RouteComponentProps<{
    segmentId: number;
    productTypeId: number;
}>;

class ProductConfiguration extends React.Component<ProductTypesProps> {
    render() {
        const { productTypeId, segmentId } = this.props.match.params;

        return (
            <div>
                <Container>
                    <h1><SegmentName segmentId={segmentId} /></h1>
                </Container>

                <ProductTypes {...this.props} />

                <TabContent className="px-3 py-4 bg-white">
                    <Container>
                        <Card className="card-tabs mb-4">
                            <CardHeader>
                                <b>Produkty</b>
                                <ToggleAllProducts />
                                <span className="float-right">
                                    <SelectedProducersLoadButtonContainer selectedProductTypeId={productTypeId} />
                                    {" "}
                                    <SelectedProducersSaveButtonContainer selectedProductTypeId={productTypeId} />
                                </span>
                            </CardHeader>
                            <CardBody>

                                <ProductVariants  selectedProductTypeId={productTypeId} />
                            </CardBody>
                        </Card>

                        <Card className="card-tabs">
                            <CardHeader>
                                <b>Témata</b> <ToggleAllTopics />
                                <span className="float-right">
                                    <ShowProductMatrixLimitedSelection {...this.props} />
                                    {" "}
                                    <SelectedTopicsLoadButtonContainer selectedProductTypeId={productTypeId} />
                                    {" "}
                                    <SelectedTopicsSaveButtonContainer selectedProductTypeId={productTypeId} isVisible={true} />
                                </span>
                            </CardHeader>
                            <CardBody>
                                <TopicSearchContainer selectedProductTypeId={productTypeId} />
                                <ShowProductMatrix {...this.props} />
                                <SelectedTopics />
                                <ShowProductMatrix {...this.props} />
                            </CardBody>
                        </Card>
                    </Container>
                </TabContent>
                {" "}
            </div>
        );
    }
}

export default ProductConfiguration;
