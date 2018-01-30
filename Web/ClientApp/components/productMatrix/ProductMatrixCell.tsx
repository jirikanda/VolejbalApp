import * as classnames from 'classnames';
import * as moment from 'moment';
import * as React from 'react';

import { CustomAttribute, ProductVariant } from '../../apimodels/ProductMatrixStore';
import { Topic } from '../../apimodels/TopicsStore';
import CellEditDialog from '../../containers/products/customAttributeEdit/CellEditDialog';
import FeedbackModal from '../../containers/products/FeedbackModal';
import getCustomAttributeAnswer from '../../helpers/customAttributeAnswer';
import { ensureTextContentShortened } from '../../helpers/dom-manipulation';
import ToggleExpanseCollapseButton from '../common/ToggleExpanseCollapseButton';
import CellEditButton from './customAttributeEdit/CellEditButton';
import FeedbackButton from './FeedbackButton';
import { CellHeight } from './ProductMatrixRow';
import { getReadDateFormat } from '../../helpers/momentHelpers';

interface ProductMatrixCellProps {
    customAttribute: CustomAttribute;
    productVariant: ProductVariant;
    isTextCollapsed: boolean;
    cellHeightOverride?: number;
    collapseButtonHeight: number;
    toggleCollapseTextContent: () => void;
    emitHeightToParent: (cellHeight: CellHeight) => void;
    topic: Topic;
    ancestorTopicsAndMe: Topic[];
    hasPermissionsToEdit: boolean;
}

interface ProductMatrixCellState {
    isTextOverflowShortened: boolean;
    expandedHeight: number;
    showFeedbackModalDialog: boolean;
    showEditModalDialog: boolean;
}

export class ProductMatrixCell extends React.Component<ProductMatrixCellProps, ProductMatrixCellState> {
    constructor(props: ProductMatrixCellProps) {
        super(props);

        this.state = {
            isTextOverflowShortened: false,
            expandedHeight: null,
            showFeedbackModalDialog: false,
            showEditModalDialog: false
        }
    }

    processTextContent = (textContentDiv: HTMLDivElement, countOfLinesToShow: number, lineHeight: number, text: string | JSX.Element) => {
        const { isTextCollapsed, emitHeightToParent, customAttribute, productVariant } = this.props;
        const { expandedHeight } = this.state;

        if (textContentDiv && !isNaN(expandedHeight)) {
            const currentTextContentDivHeight = textContentDiv.scrollHeight;
            let newExpandedHeight = expandedHeight;

            if (typeof text === "string") {
                if (isTextCollapsed) {
                    if (ensureTextContentShortened(textContentDiv, countOfLinesToShow, lineHeight, text)) {
                        if (newExpandedHeight < currentTextContentDivHeight) {
                            newExpandedHeight = currentTextContentDivHeight;
                            this.setState({ isTextOverflowShortened: true, expandedHeight: newExpandedHeight });
                        }

                        emitHeightToParent({ height: newExpandedHeight, customAttributeId: customAttribute.id, productVariantId: productVariant.id });
                    }
                } else {
                    textContentDiv.innerHTML = text;
                    textContentDiv.style.height = `${newExpandedHeight}px`;
                }
            } else {
                textContentDiv.style.height = `${lineHeight * countOfLinesToShow}em`;
            }
        }
    }

    cellCollapseButtonClicked = () => {
        this.props.toggleCollapseTextContent();
    };

    openFeedbackModal = () => this.setState({ showFeedbackModalDialog: true })

    closeFeedbackModal = () => this.setState({ showFeedbackModalDialog: false })

    openEditModal = () => this.setState({ showEditModalDialog: true })

    closeEditModal = () => this.setState({ showEditModalDialog: false })

    render() {
        const { customAttribute, productVariant, isTextCollapsed, cellHeightOverride, collapseButtonHeight, topic, ancestorTopicsAndMe, hasPermissionsToEdit } = this.props;

        const customAttributeAnswer = customAttribute.values.find(x => x.productVariantId === productVariant.id);

        const answerContent = getCustomAttributeAnswer(customAttributeAnswer);

        const lineHeight = 1.5;
        const countOfLinesToShow = 3;

        return <div
            className={classnames("product-matrix-item", customAttributeAnswer.satisfactionRateCssClass)}
            style={{ height: this.props.cellHeightOverride && `${cellHeightOverride + collapseButtonHeight}px` }}>

            <div className="product-matrix-item-text mb-1" ref={element => this.processTextContent(element, countOfLinesToShow, lineHeight, answerContent)}>
                {answerContent}
            </div>

            <div className="product-matrix-item-footer">
                <ToggleExpanseCollapseButton
                    isTextCollapsed={isTextCollapsed}
                    isVisible={this.state.isTextOverflowShortened}
                    toggleExpanseCollapseButtonClicked={this.cellCollapseButtonClicked} />
                <FeedbackButton
                    id={`${customAttribute.id}-${customAttribute.topicId}-${productVariant.id}`}
                    feedback={customAttributeAnswer.feedback}
                    feedbackButtonClicked={this.openFeedbackModal} />

                <span className="pull-right mr-2">
                    {customAttributeAnswer.satisfactionRateText}
                </span>
                <span className="product-matrix-item-validfrom float-right mr-2" title="Datum platnosti aktualizace">
                    <b>{customAttributeAnswer.wasRecentlyUpdated && customAttributeAnswer.validFrom && getReadDateFormat(customAttributeAnswer.validFrom)}</b>
                </span>

                {hasPermissionsToEdit && <CellEditButton editButtonClicked={this.openEditModal} />}
                <FeedbackModal
                    customAttribute={customAttribute}
                    customAttributeAnswer={customAttributeAnswer}
                    productVariant={productVariant}
                    showFeedbackModalDialog={this.state.showFeedbackModalDialog}
                    closeFeedbackModalDialog={this.closeFeedbackModal}
                    ancestorTopicsAndMe={ancestorTopicsAndMe} /></div>

            <CellEditDialog
                closeDialog={this.closeEditModal}
                showDialog={this.state.showEditModalDialog}
                productVariant={productVariant}
                customAttribute={customAttribute}
                ancestorTopicsAndMe={ancestorTopicsAndMe} />
        </div>
    }
}

export default ProductMatrixCell