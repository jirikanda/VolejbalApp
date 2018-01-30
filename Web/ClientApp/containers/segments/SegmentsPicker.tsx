import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as SegmentsStore from '../../store/Segments';
import * as ProductTypesStore from '../../store/ProductTypes';
import { SegmentsPicker as SegmentsPickerComponent } from '../../components/segments/SegmentsPicker';
import { SegmentVM } from '../../apimodels/SegmentsStore';

type SegmentsPickerProps =
    SegmentsStore.SegmentsState
    & typeof SegmentsStore.actionCreators;

class SegmentsPicker extends React.Component<SegmentsPickerProps> {
    componentWillMount() {
        this.props.requestSegments();
    }

    componentWillReceiveProps() {
        if (this.props.segment === null) {
            const firstSegment = this.props.segments[0];
            this.props.selectSegment(firstSegment);
        }
    }

    componentDidMount() {}

    selectedSegmentChanged = (segment: SegmentVM) => {
        this.props.selectSegment(segment);
    }

    render() {
        return <SegmentsPickerComponent {...this.props} onSelectedSegmentChanged={this.selectedSegmentChanged} />
    }
}

export default connect(
    (state: ApplicationState) => state.segments,
    {...SegmentsStore.actionCreators}
)(SegmentsPicker)