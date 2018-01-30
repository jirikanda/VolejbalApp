import * as React from 'react';
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from 'reactstrap';
import { SegmentVM } from '../../apimodels/SegmentsStore';

interface SegmentsPickerProps {
    segments: SegmentVM[];
    segment: SegmentVM;
    onSelectedSegmentChanged: (segment: SegmentVM) => void
}

interface SegmentsPickerState {
    isOpen: boolean;
}

export class SegmentsPicker extends React.Component<SegmentsPickerProps, SegmentsPickerState> {
    constructor(props: SegmentsPickerProps) {
        super(props);

        this.state = {
            isOpen: false
        }
    }

    toggle = () => this.setState((prevState) => ({ isOpen: !prevState.isOpen }));

    render() {
        return <Dropdown isOpen={this.state.isOpen} toggle={this.toggle}>
            <DropdownToggle caret>
                {this.props.segment && this.props.segment.name}
            </DropdownToggle>
            <DropdownMenu>
                {this.props.segments && this.props.segments.map(segment =>
                    <DropdownItem key={segment.id} onClick={e => this.props.onSelectedSegmentChanged(segment)}>{segment.name}</DropdownItem>)
                }
            </DropdownMenu>
        </Dropdown>
    }
}