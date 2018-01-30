import * as React from 'react';
import { Topic } from '../../apimodels/TopicsStore';
import { SatisfactionRateLevel, CustomAttribute, ProductVariant } from '../../apimodels/ProductMatrixStore';
import * as ProductMatrixStore from '../../store/ProductMatrix';
import * as moment from 'moment';
import ProductMatrixCell from './ProductMatrixCell';
import SatisfactionRateLevelFilter from '../../containers/products/SatisfactionRateLevelColorFilter'
import Slider from 'react-slick';

interface ProductMatrixRowProps {
    customAttribute: CustomAttribute;
    productVariants: ProductVariant[];
    satisfactionRateLevelFilters: ProductMatrixStore.SatisfactionRateLevelFilter[];
    topic: Topic;
    ancestorTopicsAndMe: Topic[];
    currentSlideIndex: number;
    columnsToShow: number;
    hasPermissionsToEdit: boolean;
}

interface ProductMatrixRowState {
    hasCollapsedCells: boolean;
    allCellHeights: CellHeight[];
}

export interface CellHeight {
    customAttributeId: number;
    productVariantId: number;
    height: number;
}

export class ProductMatrixRow extends React.Component<ProductMatrixRowProps, ProductMatrixRowState> {
    collapseButtonHeight = 50;

    constructor(props: ProductMatrixRowProps) {
        super(props);

        this.state = {
            hasCollapsedCells: true,
            allCellHeights: []
        };
    }

    toggleCollapseTextContent = () => {
        this.setState((prevState: ProductMatrixRowState) => ({
            hasCollapsedCells: !prevState.hasCollapsedCells
        }))
    }

    onEmitHeightToParent = (newCellHeight: CellHeight) => {
        this.setState((prevState: ProductMatrixRowState) => {
            let allCellHeights: CellHeight[];

            if (!prevState.allCellHeights
                .some(cellHeight =>
                    (cellHeight.productVariantId === newCellHeight.productVariantId) &&
                    (cellHeight.customAttributeId === newCellHeight.customAttributeId))
            ) {
                allCellHeights = prevState.allCellHeights.concat(newCellHeight);

                return {
                    allCellHeights
                }
            }
        });
    }

    render() {
        const { customAttribute, productVariants, satisfactionRateLevelFilters, topic, ancestorTopicsAndMe, currentSlideIndex, columnsToShow, hasPermissionsToEdit } = this.props;
        const { hasCollapsedCells, allCellHeights } = this.state;

        const filteredCellHeights = this.state.allCellHeights
            .filter(cellHeight => productVariants.some(variant => variant.id === cellHeight.productVariantId))
            .map(cellHeight => cellHeight.height)
        const maxCellHeight = !this.state.hasCollapsedCells && Math.max(...filteredCellHeights);

        const settings = {
            dots: false,
            infinite: false,
            draggable: false,
            speed: 0,
            slidesToShow: columnsToShow,
            slidesToScroll: 1,
            arrows: false,
            slickGoTo: currentSlideIndex,
            asNavFor: '.product-matrix-header-slider',
            variableWidth: true
        };

        // answers for product variants filtered by rate level
        const cells = productVariants
            .filter(productVariant => satisfactionRateLevelFilters.some(filter => filter.satisfactionRateLevels.length === 0))
            .map((productVariant, index) =>
                <div key={productVariant.id} style={{ visibility: (index >= (columnsToShow + currentSlideIndex)) ? "hidden" : "visible" }}>
                    <ProductMatrixCell
                        customAttribute={customAttribute}
                        productVariant={productVariant}
                        isTextCollapsed={hasCollapsedCells}
                        toggleCollapseTextContent={this.toggleCollapseTextContent}
                        emitHeightToParent={this.onEmitHeightToParent}
                        cellHeightOverride={maxCellHeight}
                        collapseButtonHeight={this.collapseButtonHeight}
                        topic={topic}
                        ancestorTopicsAndMe={ancestorTopicsAndMe}
                        hasPermissionsToEdit={hasPermissionsToEdit}/>
                </div>)

        const isSatisfactionRateLevelFilterVisible = customAttribute.values
            .some(custuomAttributeValue => custuomAttributeValue.satisfactionRateCssClass && custuomAttributeValue.satisfactionRateCssClass.length > 0);

        return <div className="product-matrix-row">
            <div className="product-matrix-item" style={{ height: maxCellHeight ? `${maxCellHeight + this.collapseButtonHeight}px` : void 0 }}>
                {customAttribute.name}
                <SatisfactionRateLevelFilter isSatisfactionRateLevelFilterVisible={isSatisfactionRateLevelFilterVisible} customAttribute={customAttribute} />
                {
                    customAttribute.displayIncludedInQuickSelection
                        && <i className="fa fa-star-o included-in-quick-selection" aria-hidden="true"></i>
                }
            </div>
            <Slider className="product-matrix-row-slider" {...settings}>{cells}</Slider>
        </div>
    }
}