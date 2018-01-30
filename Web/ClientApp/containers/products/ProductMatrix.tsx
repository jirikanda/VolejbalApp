import * as React from "react";
import * as ReactDOM from "react-dom";
import { RouteComponentProps } from "react-router-dom";
import { connect } from "react-redux";
import * as ProductMatrixStore from "../../store/ProductMatrix";
import * as ProductVariantDocumentsStore from "../../store/ProductVariantDocuments";
import { ApplicationState } from "../../store";
import ProductMatrixHeader from "../../components/productMatrix/ProductMatrixHeader";
import { ProductMatrixRow } from "../../components/productMatrix/ProductMatrixRow";
import { ProductMatrixBody } from "../../components/productMatrix/ProductMatrixBody";
import { Topic } from "./../../apimodels/TopicsStore";
import { CustomAttribute, ProductVariant } from "./../../apimodels/ProductMatrixStore";
import ScrollHandler from '../../components/common/ScrollHandler';

type ProductMatrixProps = ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators
    & typeof ProductVariantDocumentsStore.actionCreators;

class ProductMatrix extends React.Component<ProductMatrixProps> {
    handleScroll = (event: UIEvent, element: Element) => {
        const breadcrumb = document.querySelector(".breadcrumb");

        if ((breadcrumb.clientHeight < window.scrollY) && !element.classList.contains("fixed")) {
            element.classList.add("fixed");

            const breadCrumbStyle = window.getComputedStyle(breadcrumb);
            const breadcrumbHeight = ['height', 'margin-bottom']
                .map((key) => parseInt(breadCrumbStyle.getPropertyValue(key), 10))
                .reduce((previous, current) => previous + current);

            (document.querySelector(".product-matrix-table") as HTMLElement).style.paddingTop = `${breadcrumbHeight + element.clientHeight}px`;
        }

        if ((breadcrumb.clientHeight > window.scrollY) && element.classList.contains("fixed")) {
            element.classList.remove("fixed");
            (document.querySelector(".product-matrix-table") as HTMLElement).style.paddingTop = "0px";
        }
    }

    render() {
        const { productVariants, selectedTopics, setCurrentSlideIndex, setColumnsToShow } = this.props;

        return (
            <div className="container-fluid">
                <div className="product-matrix-table">
                    <ScrollHandler onScroll={this.handleScroll}>
                        <ProductMatrixHeader
                            {...this.props}
                            hasSelectedProductVariantsOrTopics={productVariants.length > 0 && selectedTopics.length > 0}
                            onAfterSliderChange={setCurrentSlideIndex}
                            onColumnsToShowChanged={setColumnsToShow} />
                    </ScrollHandler>
                    <ProductMatrixBody {...this.props} />
                </div>
            </div>
        );
    }
}

export default connect(
    (state: ApplicationState) => state.productMatrix,
    {...ProductMatrixStore.actionCreators, ...ProductVariantDocumentsStore.actionCreators}
)(ProductMatrix);
