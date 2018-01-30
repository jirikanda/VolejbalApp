import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as TopicStore from '../../store/Topics';
import SelectedTopicsSaveButton from '../../components/common/SelectedTopicsSaveButton';

type SelectedTopicsSaveButtonContainerProps =
    TopicStore.TopicState
    & typeof TopicStore.actionCreators
    & { selectedProductTypeId: number; isVisible: boolean }

class SelectedTopicsSaveButtonContainer extends React.Component<SelectedTopicsSaveButtonContainerProps> {
    render() {
        return <>{this.props.isVisible && <SelectedTopicsSaveButton {...this.props} />}</>
    }
}

const mapStateToProps = (state: ApplicationState, props: { selectedProductTypeId?: number }) => {
    return {
        ...state.topics,
        selectedProductTypeId: state.productTypes.productType ? state.productTypes.productType.id : props.selectedProductTypeId
    }
}

export default connect(mapStateToProps, TopicStore.actionCreators)(SelectedTopicsSaveButtonContainer)