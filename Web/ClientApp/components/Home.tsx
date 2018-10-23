import * as React from 'react'
import Counter from '../containers/Counter'
import Configuration from '../configuration'
import { withStyles, WithStyles } from '@material-ui/core/styles';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import { NavLink, withRouter, RouteComponentProps } from 'react-router-dom';
import { compose } from 'redux';

const styles = theme => ({
    root: {
        ...theme.mixins.gutters(),
        paddingTop: theme.spacing.unit * 2,
        paddingBottom: theme.spacing.unit * 2,
    },
});

const Home = (props: RouteComponentProps<{}> & WithStyles<typeof styles>) => {
    const { classes } = props;

    return <>
        <Paper className={classes.root} elevation={1}>
            <Counter />        
            <Typography variant="headline" component="h3">
                Web configuration
            </Typography>
            <Typography component="p">
                {JSON.stringify(Configuration)}
            </Typography>
            <Typography component="p">
                <NavLink to='/hello-world'>Hello world</NavLink>
            </Typography>           
        </Paper>
    </>;
}

export default compose(withRouter, withStyles(styles))(Home);