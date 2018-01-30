import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as TopicStore from '../../store/Topics';
import * as ProductVariantStore from '../../store/ProductVariants';
import TopicSearch from './../../components/topics/topicSearch/TopicSearch';

type TopicSearchContainerProps =
    TopicStore.TopicState
    & ProductVariantStore.ProductVariantsState
    & typeof TopicStore.actionCreators
    & { selectedProductTypeId: number }

class TopicSearchContainer extends React.Component<TopicSearchContainerProps> {
    render() {
        return <TopicSearch {...this.props} isSearchingAllowed={this.props.selectedProductVariants.length > 0} />
    }
}

const mapStateToProps = (state: ApplicationState, props: { selectedProductTypeId: number }) => ({ ...state.topics, ...state.productVariants })

export default connect(mapStateToProps, TopicStore.actionCreators)(TopicSearchContainer);