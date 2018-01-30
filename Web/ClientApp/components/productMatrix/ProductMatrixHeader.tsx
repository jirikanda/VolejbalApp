import * as React from 'react';
import { ProductVariant } from '../../apimodels/ProductMatrixStore';
import { ProductMatrixHeaderItem } from './ProductMatrixHeaderItem';
import { Button } from 'reactstrap';
import { ProductType } from '../../apimodels/SegmentsStore';
import Topics from '../../containers/topics/Topics';
import ProductVariantsModalDialog from './ProductVariantsModalDialog';
import TopicsModalDialog from './TopicsModalDialog';
import CancelSatisfactionRateLevelLink from '../../containers/products/CancelSatisfactionRateLevelLink';
import ProductVariantDocumentsDialog from './productVariantDocuments/ProductVariantDocumentsDialog';
import { ProductVariantDocument } from '../../store/ProductVariantDocuments';
import ProductVariantDocuments from '../../containers/products/ProductVariantDocuments';
import Slider from 'react-slick';

interface ProductMatrixHeaderProps {
    productVariants: ProductVariant[];
    productVariantsToHideByRateFilter: ProductVariant[];
    selectedProductType: ProductType;
    requestCustomAttributes: (selectedProductTypeId: number) => void;
    requestCustomAttributesQuickSelection: (productTypeId: number) => void;
    hasSelectedProductVariantsOrTopics: boolean;
    unselectProductVariant: (productVariant: ProductVariant) => void;
    requestProductVariantDocuments: (productVariant: ProductVariant) => void;
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument) => void;
    saveProductVariantDocumentsMetadata: (productVariantDocument: ProductVariantDocument, newValidFrom: Date, newValidTo?: Date) => void;
    changeProductVariantDocumentsOrder: (hoverIndex: number, currentItemId: number) => void;
    isQuickSelection: boolean;
    onAfterSliderChange: (currentSlide: number) => void;
    onColumnsToShowChanged: (columnsToShow: number) => void;
    currentSlideIndex: number;
    columnsToShow: number;
}

interface ProductMatrixHeaderState {
    isTopicsModalOpen: boolean;
    isProductVariantsModalOpen: boolean;
    isProductVariantDocumentsModalOpen: boolean
}

export default class ProductMatrixHeader extends React.Component<ProductMatrixHeaderProps, ProductMatrixHeaderState> {
    constructor(props: ProductMatrixHeaderProps) {
        super(props);

        this.state = {
            isTopicsModalOpen: false,
            isProductVariantsModalOpen: false,
            isProductVariantDocumentsModalOpen: false
        }
    }

    toggleProductVariantDocumentsModal = () => {
        this.setState(prevState => ({ isProductVariantDocumentsModalOpen: !prevState.isProductVariantDocumentsModalOpen }));
    }

    toggleTopicsModal = () => {
        this.setState(prevState => ({ isTopicsModalOpen: !prevState.isTopicsModalOpen }));
    }

    toggleProductVariantsModal = () => {
        this.setState(prevState => ({ isProductVariantsModalOpen: !prevState.isProductVariantsModalOpen }));
    }

    requestCustomAttributesAndClose(closeModalFunction: Function) {
        this.props.isQuickSelection ? this.props.requestCustomAttributesQuickSelection(this.props.selectedProductType.id) : this.props.requestCustomAttributes(this.props.selectedProductType.id);
        if (this.props.hasSelectedProductVariantsOrTopics) {
            closeModalFunction();
        }
    }

    calculateColumnsToShow = () => {
        let headerWidth = document.querySelector(".product-matrix-header").clientWidth;
        const headerItemWidth = document.querySelector(".product-matrix-header-item").clientWidth;

        let columnsToShow = 0;
        while ((headerWidth = (headerWidth - headerItemWidth)) > headerItemWidth) {
            columnsToShow += 1;
        }
        return columnsToShow;
    }

    componentDidMount() {
        const columnsToShow = this.calculateColumnsToShow();

        if (this.props.columnsToShow !== columnsToShow && columnsToShow !== 0) {
            this.props.onColumnsToShowChanged(columnsToShow);
        }
    }

    componentDidUpdate() {
        const columnsToShow = this.calculateColumnsToShow();

        if (this.props.columnsToShow !== columnsToShow && columnsToShow !== 0) {
            this.props.onColumnsToShowChanged(columnsToShow);
        }
    }

    render() {
        const { productVariants, selectedProductType, unselectProductVariant, productVariantsToHideByRateFilter, isQuickSelection, currentSlideIndex, columnsToShow } = this.props;
        const { isTopicsModalOpen, isProductVariantsModalOpen, isProductVariantDocumentsModalOpen } = this.state;

        const settings = {
            dots: false,
            infinite: false,
            speed: 0,
            slidesToShow: columnsToShow,
            slidesToScroll: 1,
            arrows: true,
            afterChange: (currentSlide: number) => this.props.onAfterSliderChange(currentSlide),
            asNavFor: '.product-matrix-row-slider',
            variableWidth: true
        };

        const productVariantsElements: JSX.Element[] = productVariants
            .filter(productVariant =>
                productVariantsToHideByRateFilter.filter(productVariantToHide => productVariantToHide.id === productVariant.id).length === 0)
            .map((productVariant, index) =>
                <div key={productVariant.id} style={{ visibility: (index >= (this.props.columnsToShow + currentSlideIndex)) ? "hidden" : "visible" }}><ProductMatrixHeaderItem
                    {...this.props}
                    productVariant={productVariant}
                    onUnselectProductVariant={unselectProductVariant}
                    toggleProductVariantDocumentsDialog={this.toggleProductVariantDocumentsModal} /></div>);

        return <div className="product-matrix-header">
            <div className="product-matrix-header-row">
                <div className="product-matrix-header-item">
                    <Button color="primary" className="mr-2" onClick={event => this.toggleTopicsModal()} disabled={isQuickSelection}>+ Téma</Button>
                    <Button color="primary" onClick={event => this.toggleProductVariantsModal()}>+ Produkt</Button>
                    <CancelSatisfactionRateLevelLink />
                </div>
                <Slider className="product-matrix-header-slider" {...settings}>{productVariantsElements}</Slider>
            </div>
            <TopicsModalDialog
                selectedProductTypeId={selectedProductType.id}
                applyAndClose={() => this.requestCustomAttributesAndClose(() => this.toggleTopicsModal())}
                isTopicsModalOpen={isTopicsModalOpen}
                toggleTopicsModal={this.toggleTopicsModal} />
            <ProductVariantsModalDialog
                selectedProductTypeId={selectedProductType.id}
                applyAndClose={() => this.requestCustomAttributesAndClose(() => this.toggleProductVariantsModal())}
                isProductVariantsModalOpen={isProductVariantsModalOpen}
                toggleProductVariantsModal={this.toggleProductVariantsModal} />
            <ProductVariantDocuments
                {...this.props}
                isProductVariantDocumentsModalOpen={isProductVariantDocumentsModalOpen}
                toggleProductVariantDocumentsModal={this.toggleProductVariantDocumentsModal} />
        </div>
    }
}