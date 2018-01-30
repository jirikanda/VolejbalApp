import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as TopicStore from '../../store/Topics';
import TreeBox from '../../components/common/TreeBox';
type ProductVariantsProps =
    TopicStore.TopicState
    & typeof TopicStore.actionCreators
    & {
        selectedProductTypeId: number;
        shouldPreserveSelection: boolean;
    }

class Topics extends React.Component<ProductVariantsProps> {
    componentWillMount() {
        const {shouldPreserveSelection, selectedProductTypeId, unselectAllTopics, requestTopics} = this.props;

        if(shouldPreserveSelection) {
            return;
        }
        if (selectedProductTypeId) {
            requestTopics(selectedProductTypeId);
        }
        if (!location.pathname.includes("product-matrix")) {
            unselectAllTopics();
        }
    }

    componentWillReceiveProps(props: ProductVariantsProps) {
        if (props.selectedProductTypeId !== this.props.selectedProductTypeId) {
            this.props.requestTopics(props.selectedProductTypeId);
        }
    }

    render() {
        return <TreeBox
            items={this.props.topics}
            checkedItems={this.props.selectedTopics}
            onCheckItem={this.props.selectTopic}
            onUncheckItem={this.props.unselectTopic}
            onCheckItems={this.props.selectTopics}
            onUncheckItems={this.props.unselectTopics}
            onSelectItem={this.props.showChildrenTopics}
            visibleItems={this.props.visibleTopics}
            itemsPath={this.props.topicsPathSelected}
        />
    }
}

export default connect((state: ApplicationState, props: { selectedProductTypeId: number }) => state.topics, TopicStore.actionCreators)(Topics)